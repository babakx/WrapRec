using CsvHelper.Configuration;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using WrapRec.Evaluation;
using WrapRec.Readers;
using WrapRec.Readers.NewReaders;
using WrapRec.Recommenders;
using WrapRec.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using System.IO;
using LinqLib.Sequence;

namespace WrapRec.Experiments
{
    public class Journal2014Experiments
    {

        string _musicUsersPath = @"D:\Data\Datasets\Amazon\Old\ECIR 2014 Dataset\music_selected.users";
        string _ecirTrain = @"D:\Data\Datasets\Amazon\Old\books_selected-ex3-train0.75.libfm";
        string _ecirTest = @"D:\Data\Datasets\Amazon\Old\books_selected-ex3-test0.75.libfm";

        public void Run(int testNum = 3)
        {
            var startTime = DateTime.Now;

            switch (testNum)
            { 
                case(1):
                    TestAmazonDatasetSingle();
                    break;
                case(2):
                    TestAmazonDatasetSingleNewModel();
                    break;
                case(3):
                    TestAmazonCrossDomainBook();
                    break;
                case(4):
                    TestCrossDomain();
                    break;
                case(5):
                    ReportStatistics();
                    break;
                case(6):
                    TestAmazonCrossDomainMusic();
                    break;
                case(7):
                    CreateDatasetsFromOriginalDataset();
                    break;
                case(8):
                    TestNewDataset();
                    break;
                case(9):
                    TestAuxDataSize();
                    break;
                case(10):
                    SplitAmazon();
                    break;
                case(11):
                    TestAmazonAllDomains2();
                    break;
                case(12):
                    ShuffleAndRecreate();
                    break;
                default:
                    break;
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("Execution time: {0} sec", (int) duration.TotalSeconds);
        }

        public void TestAmazonDatasetSingle()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };
            
            var trainReader = new CsvReader<ItemRating>(Paths.AmazonBooksTrain75, config, new ItemRatingMap());
            var testReader = new CsvReader<ItemRating>(Paths.AmazonBooksTest25, config, new ItemRatingMap());
            var dataset = new Dataset<ItemRating>(trainReader, testReader);

            // step 2: recommender
            var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();            
        }

        public void TestAmazonDatasetSingleNewModel()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var trainContainer = new DataContainer();
            var testContainer = new DataContainer();

            
            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config);

            trainReader.LoadData(trainContainer);
            testReader.LoadData(testContainer);

            

            var dataset = new ItemRatingDataset(trainContainer, testContainer);

            //var featureBuilder = new LibFmFeatureBuilder();

            // step 2: recommender
            var recommender = new LibFmTrainTester();

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();
        }
        
        public void TestAmazonAllDomains2()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new DataContainer();

            var bookReader = new CsvReader(Paths.AmazonBooksRatings, config);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config);

            bookReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);

            var splitter = new RatingSimpleSplitter(container, 0.25f);
            var startTime = DateTime.Now;
            var recommender = new LibFmTrainTester();
            //var recommender = new MediaLiteRatingPredictor(new MatrixFactorization());

            // step3: evaluation
            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());
            ep.Run();

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("RMSE\t{0}\nDuration\t{1}", ctx["RMSE"], (int)duration.TotalMilliseconds);
        }

        public void TestAmazonAllDomains()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music", true);
            var dvdDomain = new Domain("dvd", true);
            var videoDomain = new Domain("video", true);

            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config, bookDomain);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);
            //tempReader.LoadData(container);

            container.PrintStatistics();
            //musicDomain.CacheUserData();
            
            container.ShuffleDomains();

            var splitter = new CrossDomainSimpleSplitter(container);
            //var splitter = new RatingSimpleSplitter(container);

            var numAuxRatings = new int[1] { 0 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                ITrainTester<ItemRating> recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, num);
                    //featureBuilder.LoadCachedUserData(_musicUsersPath);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonCrossDomainBook()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config, bookDomain);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);
            //tempReader.LoadData(container);
            
            //container.ShuffleDomains();
            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container);
            //var splitter = new RatingSimpleSplitter(container);

            var numAuxRatings = new int[4] { 0, 1, 2, 3 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;
                
                // step 2: recommender
                ITrainTester<ItemRating> recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, num);
                    //featureBuilder.LoadCachedUserData(_musicUsersPath);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));
                
                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonCrossDomainVideo()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book");
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video", true);

            var trainReader = new CsvReader(Paths.AmazonVideoTrain75, config, videoDomain);
            var testReader = new CsvReader(Paths.AmazonVideoTest25, config, videoDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var bookReader = new CsvReader(Paths.AmazonBooksRatings, config, bookDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            bookReader.LoadData(container);
            //tempReader.LoadData(container);
            //container.ShuffleDomains();
            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container);
            //var splitter = new RatingSimpleSplitter(container);

            var numAuxRatings = new int[4] { 0, 1, 2, 3 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                ITrainTester<ItemRating> recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(videoDomain, num);
                    //featureBuilder.LoadCachedUserData(_musicUsersPath);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonCrossDomainDvd()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book");
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd", true);
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonDvdTrain75, config, dvdDomain);
            var testReader = new CsvReader(Paths.AmazonDvdTest25, config, dvdDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var bookReader = new CsvReader(Paths.AmazonBooksRatings, config, bookDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            bookReader.LoadData(container);
            videoReader.LoadData(container);
            //tempReader.LoadData(container);
            //container.ShuffleDomains();
            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container);
            //var splitter = new RatingSimpleSplitter(container);

            var numAuxRatings = new int[4] { 0, 1, 2, 3 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                ITrainTester<ItemRating> recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(dvdDomain, num);
                    //featureBuilder.LoadCachedUserData(_musicUsersPath);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonCrossDomainMusic()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book");
            var musicDomain = new Domain("music", true);
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonMusicTrain75, config, musicDomain);
            var testReader = new CsvReader(Paths.AmazonMusicTest25, config, musicDomain, true);
            var bookReader = new CsvReader(Paths.AmazonBooksRatings, config, bookDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            bookReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);
            //tempReader.LoadData(container);
            //container.ShuffleDomains();
            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container);
            //var splitter = new RatingSimpleSplitter(container);

            var numAuxRatings = new List<int> { 0, 1, 2, 3 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                ITrainTester<ItemRating> recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(musicDomain, num);
                    //featureBuilder.LoadCachedUserData(_musicUsersPath);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void CreateDatasetsFromOriginalDataset()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book");
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var bookReader = new CsvReader(Paths.AmazonAllBookRatings, config, bookDomain);
            var musicReader = new CsvReader(Paths.AmazonAllMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonAllDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonAllVideoRatings, config, videoDomain);

            bookReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);

            var output = container.Users.Values.Where(u => 
            { 
                var counts = u.Ratings.GroupBy(r => r.Domain).Select(g => g.Count());
                return counts.All(c => c >= 1 && c <= 20) && (counts.Count() > 3);
            })
            //.Select(u => new { UserId = u.Id, Counts = u.Ratings.GroupBy(r => r.Domain.Id).Select(g => g.Count().ToString()).Aggregate((a,b) => a + " " + b) })
            //.Select(a => a.UserId + "," + a.Counts);

            .SelectMany(u => u.Ratings.Where(r => r.Domain == musicDomain))
            //.SelectMany(u => u.Ratings.GroupBy(r => r.Item.Id).Select(g => g.Take(1).Single()))
            .Select(r => r.ToString());

            Console.WriteLine("Writing...");
            var header = new string[] { "UserId,ItemId,Rating" };

            // selected1: only music between 5 to 20
            // selected2: only music between 1 to 20
            // selected3: only music between 2 to 20
            // selected4: all domains with ratings between 1 to 20
            File.WriteAllLines("music_selected4.csv", header.Concat(output));

            //container.PrintStatistics();
        }

        public void TestNewDataset()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var bookReader = new CsvReader("books_selected4.csv", config, bookDomain);
            //var trainReader = new CsvReader("books_selected1_train.csv", config, bookDomain);
            //var testReader = new CsvReader("books_selected1_test.csv", config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonAllMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonAllDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonAllVideoRatings, config, videoDomain);

            bookReader.LoadData(container);
            //trainReader.LoadData(container);
            //testReader.LoadData(container);
            musicReader.LoadData(container);
            //dvdReader.LoadData(container);
            //videoReader.LoadData(container);
            
            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);
            //splitter.SaveSplitsAsCsv("books_selected1_train.csv", "books_selected1_test.csv");

            //return;
            //var splitter = new RatingSimpleSplitter(container, 0.25f);

            var numAuxRatings = new List<int> { 0, 1, 2, 3, 5, 7, 10 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                LibFmTrainTester recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, num);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(recommender.RMSE.ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAuxDataSize()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var bookReader = new CsvReader("books_selected4.csv", config, bookDomain);
            //var trainReader = new CsvReader("books_selected1_train.csv", config, bookDomain);
            //var testReader = new CsvReader("books_selected1_test.csv", config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonAllMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonAllDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonAllVideoRatings, config, videoDomain);

            bookReader.LoadData(container);
            //trainReader.LoadData(container);
            //testReader.LoadData(container);
            musicReader.LoadData(container);
            //dvdReader.LoadData(container);
            //videoReader.LoadData(container);

            container.PrintStatistics();

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);
            //splitter.SaveSplitsAsCsv("books_selected1_train.csv", "books_selected1_test.csv");


            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                var startTime = DateTime.Now;

                musicDomain.ActivateData(0.1f);

                // step 2: recommender
                LibFmTrainTester recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, 10);
                recommender = new LibFmTrainTester(experimentId: i.ToString(), featureBuilder: featureBuilder);


                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                rmse.Add(recommender.RMSE.ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, rmse[i], mae[i], durations[i]);
            }
        }


        public void TestContainer(CrossDomainDataContainer container)
        {
            var us = container.Users.Values.Where(u => u.Id == "2305").Single();
            
            Console.WriteLine(us.Ratings.Where(r => r.Domain.Id == "book").Count());                        // 33
            Console.WriteLine(us.Ratings.Where(r => r.Domain.Id == "music").Count());                       // 3
            Console.WriteLine(us.Ratings.Where(r => r.IsTest == false).Count());                            // 29
            Console.WriteLine(us.Ratings.Where(r => r.IsTest == false && r.Domain.Id == "book").Count());   // 26
            Console.WriteLine(us.Ratings.Where(r => r.IsTest == true && r.Domain.Id == "book").Count());    // 7
        }


        public void TestCrossDomain()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var domain1 = new Domain("domain1", true);
            var domain2 = new Domain("domain2");

            var trainReader = new CsvReader(Paths.TestDomain1Train, config, domain1);
            var auxReader = new CsvReader(Paths.TestDomain2, config, domain2);
            var testReader = new CsvReader(Paths.TestDomain1Test, config, domain1, true);

            trainReader.LoadData(container);
            auxReader.LoadData(container);
            testReader.LoadData(container);


            var dataset = new ItemRatingDataset(container);

            var featureBuilder = new CrossDomainLibFmFeatureBuilder(domain1);

            // step 2: recommender
            var recommender = new LibFmTrainTester(featureBuilder: featureBuilder);

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();

            // featureBuilder.Mapper.OriginalIDs.ToList().ForEach(Console.WriteLine);
            // featureBuilder.Mapper.InternalIDs.ToList().ForEach(Console.WriteLine);
        }


        public void ReportStatistics()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config, bookDomain);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);

            container.WriteHistogram(Paths.AmazonProcessedPath);
        }

        public void SplitAmazon()
        {
            //FileHelper.SplitLines(Paths.AmazonDvdRatings, Paths.AmazonDvdTrain75, Paths.AmazonDvdTest25, 0.75, true, true);
            FileHelper.SplitLines(Paths.AmazonVideoRatings, Paths.AmazonVideoTrain75, Paths.AmazonVideoTest25, 0.75, true, true);
        }

        public void ShuffleAndRecreate()
        {
            var lines = new List<string>();
            
            lines.AddRange(File.ReadAllLines(Paths.AmazonBooksRatings).Skip(1));
            lines.AddRange(File.ReadAllLines(Paths.AmazonMusicRatings).Skip(1));
            lines.AddRange(File.ReadAllLines(Paths.AmazonDvdRatings).Skip(1));
            lines.AddRange(File.ReadAllLines(Paths.AmazonVideoRatings).Skip(1));

            var header = new string[] { "UserId,ItemId,Rating" };

            int count = lines.Count / 4;

            lines = lines.Shuffle().ToList();

            File.WriteAllLines(Paths.AmazonShuffle1, header.Concat(lines.Take(count)));
            File.WriteAllLines(Paths.AmazonShuffle2, header.Concat(lines.Skip(count).Take(count)));
            File.WriteAllLines(Paths.AmazonShuffle3, header.Concat(lines.Skip(count * 2).Take(count)));
            File.WriteAllLines(Paths.AmazonShuffle4, header.Concat(lines.Skip(count * 3)));

            FileHelper.SplitLines(Paths.AmazonShuffle1, Paths.AmazonShuffle1 + ".75", Paths.AmazonShuffle1 + ".25", 0.75, true, true);
            FileHelper.SplitLines(Paths.AmazonShuffle2, Paths.AmazonShuffle2 + ".75", Paths.AmazonShuffle2 + ".25", 0.75, true, true);
            FileHelper.SplitLines(Paths.AmazonShuffle3, Paths.AmazonShuffle3 + ".75", Paths.AmazonShuffle3 + ".25", 0.75, true, true);
            FileHelper.SplitLines(Paths.AmazonShuffle4, Paths.AmazonShuffle4 + ".75", Paths.AmazonShuffle4 + ".25", 0.75, true, true);
        }
    }
}
