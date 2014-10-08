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

namespace WrapRec.Experiments
{
    public class Journal2014Experiments
    {

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
                    TestAmazonCrossDomain();
                    break;
                case(4):
                    TestCrossDomain();
                    break;
                case(5):
                    ReportStatistics();
                    break;
                default:
                    break;
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("Execution time: {0} sec", duration.Seconds);
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

        public void TestAmazonCrossDomain(int x = 1)
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
            //dvdReader.LoadData(container);
            //videoReader.LoadData(container);

            //container.PrintStatistics();

            var dataset = new ItemRatingDataset(container);

            var xx = new int[1] { 0};

            var rmse = new List<string>();
            var mae = new List<string>();

            foreach (var item in xx)
            {
                var featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, item);

                // step 2: recommender
                var recommender = new LibFmTrainTester(featureBuilder: featureBuilder);

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, dataset);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());
            }

            Console.WriteLine("RMSE");
            rmse.ForEach(Console.WriteLine);

            Console.WriteLine("MAE");
            mae.ForEach(Console.WriteLine);

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

    }
}
