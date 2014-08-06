using RF2.Entities;
using RF2.Evaluation;
using RF2.Readers;
using RF2.Recommenders;
using RF2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using RF2.Utilities;
using MyMediaLite.Data;

namespace RF2.Experiments
{
    public class Recsys2014Experiments
    {

        public void Run(int testNum = 8)
        {
            switch (testNum)
            {
                case (1):
                    Test();
                    break;
                case(2):
                    CreateClustersMovieLens();
                    break;
                case(3):
                    CreateClustersEpinion();
                    break;
                case(4):
                    TestMovieLensWithClusters();
                    break;
                case(5):
                    TestEpinionClusters();
                    break;
                case(6):
                    CreateClustersAmazon();
                    break;
                case(7):
                    SplitFiles();
                    break;
                case(8):
                    TestAmazonWithClusters();
                    break;
                case(9):
                    TestNmfClusting();
                    break;
                case(10):
                    TestAmazonWithNmfCluster();
                    break;
                case(11):
                    CreateNmfClustersAmazon();
                    break;
                case(12):
                    CreateAuxClusters();
                    break;
                default:
                    break;
            }        
        }

        public void CreateAuxClusters()
        {
            for (int i = 2; i < 15; i += 2)
            {
                var xdUserClusterer = new XDomainClusterer(Paths.AmazonBooksUsersCluster + i, Paths.MovieLens1MUsersCluster + i, "ml");
                var xdItemClusterer = new XDomainClusterer(Paths.AmazonBooksItemsCluster + i, Paths.MovieLens1MItemsCluster + i, "ml");

                xdUserClusterer.WriteClusterMemberships();
                xdItemClusterer.WriteClusterMemberships();
            }
        }
        
        public void CreateNmfClustersAmazon()
        {
            var reader = new AmazonReader(Paths.AmazonBooksRatings);
            var dataset = new Dataset<ItemRating>(reader);

            var userMapping = new Mapping();
            var itemMapping = new Mapping();

            var data = dataset.AllSamples.Select(ir => new
            {
                UserId = userMapping.ToInternalID(ir.User.Id),
                ItemId = itemMapping.ToInternalID(ir.Item.Id),
                Rating = Convert.ToDouble(ir.Rating)
            }).ToList();

            // users
            int i = 0;
            var uOut = File.ReadAllLines(Paths.AmazonBooksUsersCluster + ".lf").Select(l => 
            {
                var values = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => Convert.ToDouble(v)).ToList();

                var maxIndex = values.IndexOf(values.Max());

                return new { UserId = userMapping.ToOriginalID(i++), ClusterId = maxIndex };
            }).Select(uc => string.Format("{0},{1}", uc.UserId, uc.ClusterId));

            File.WriteAllLines(Paths.AmazonBooksUsersCluster + ".nmf.u", uOut);

            // items
            int j = 0;
            var iOut = File.ReadAllLines(Paths.AmazonBooksUsersCluster + ".rf").Select(l =>
            {
                var values = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => Convert.ToDouble(v)).ToList();

                var maxIndex = values.IndexOf(values.Max());

                return new { ItemId = itemMapping.ToOriginalID(j++), ClusterId =  maxIndex };
            }).Select(ic => string.Format("{0},{1}", ic.ItemId, ic.ClusterId));

            File.WriteAllLines(Paths.AmazonBooksUsersCluster + ".nmf.i", iOut);
        }

        public void TestAmazonWithNmfCluster()
        {
            var trainReader = new AmazonReader(Paths.AmazonBooksTrain75, Paths.AmazonBooksUsersCluster + ".nmf.u", Paths.AmazonBooksUsersCluster + ".nmf.i");
            var testReader = new AmazonReader(Paths.AmazonBooksTest25, Paths.AmazonBooksUsersCluster + ".nmf.u", Paths.AmazonBooksUsersCluster + ".nmf.i");

            var dataset = new Dataset<ItemRatingWithClusters>(trainReader, testReader);

            var recommender = new LibFmTrainTester();

            var context = new EvalutationContext<ItemRating>(recommender, dataset);

            var ep = new EvaluationPipeline<ItemRating>(context);
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();
        }

        public void SplitFiles()
        {
            FileHelper.SplitLines(Paths.AmazonMusicRatings, Paths.AmazonMusicTrain75, Paths.AmazonMusicTest25, 0.75, true, true);
        }

        public void TestNmfClusting()
        {
            var reader = new AmazonReader(Paths.AmazonBooksRatings);
            var dataset = new Dataset<ItemRating>(reader);
            var clusterer = new Clusterer(dataset);

            clusterer.ClusterNmf(5, Paths.AmazonBooksUsersCluster);
        }

        public void CreateClustersMovieLens()
        { 
            var reader = new MovieLensReader(Paths.MovieLens1M);
            var dataset = new Dataset<ItemRating>(reader);
            var clusterer = new Clusterer(dataset);

            for (int i = 2; i < 15; i += 2)
            {
                clusterer.WriteUsersCluster(Paths.MovieLens1MUsersCluster + i + ".csv", i, 5);
                clusterer.WriteItemsCluster(Paths.MovieLens1MItemsCluster + i + ".csv", i, 5);
            }
        }

        public void CreateClustersEpinion()
        {
            var reader = new EpinionReader(Paths.EpinionRatings);
            var dataset = new Dataset<ItemRating>(reader);
            var clusterer = new Clusterer(dataset);

            for (int i = 2; i < 15; i += 2)
            {
                clusterer.WriteUsersCluster(Paths.EpinionUsersCluster + i + ".csv", i, 5);
                clusterer.WriteItemsCluster(Paths.EpinionItemsCluster + i + ".csv", i, 5);
            }
        }

        public void CreateClustersAmazon()
        {
            var reader = new AmazonReader(Paths.AmazonMusicRatings);
            var dataset = new Dataset<ItemRating>(reader);
            var clusterer = new Clusterer(dataset);

            for (int i = 14; i < 15; i += 2)
            {
                clusterer.WriteUsersCluster(Paths.AmazonMusicUsersCluster + i + ".csv", i, 5);
                clusterer.WriteItemsCluster(Paths.AmazonMusicItemsCluster + i + ".csv", i, 5);
            }
        }

        public void TestAmazonWithClusters()
        {
            // Todo: instead of loading the dataset every time add a updateCluster method to the Dataset<ItemRatingWithCluster>
            List<string> rmses = new List<string>();
            List<string> maes = new List<string>();

            for (int i = 2; i < 13; i += 2)
            {
                string usersClusterFile = Paths.AmazonMusicUsersCluster + i + ".csv";
                string itemsClusterFile = Paths.AmazonMusicItemsCluster + i + ".csv";

                AmazonReader trainReader, testReader;

                if (i == 0)
                {
                    trainReader = new AmazonReader(Paths.AmazonMusicTrain75);
                    testReader = new AmazonReader(Paths.AmazonMusicTest25);
                }
                else
                {
                    trainReader = new AmazonReader(Paths.AmazonMusicTrain75, usersClusterFile, itemsClusterFile, "b", true);
                    testReader = new AmazonReader(Paths.AmazonMusicTest25, usersClusterFile, itemsClusterFile, "b", true);
                }

                var dataset = new Dataset<ItemRatingWithClusters>(trainReader, testReader);

                var recommender = new LibFmTrainTester(i);

                var context = new EvalutationContext<ItemRating>(recommender, dataset);

                var ep = new EvaluationPipeline<ItemRating>(context);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());

                ep.Run();

                rmses.Add(context["RMSE"].ToString());
                maes.Add(context["MAE"].ToString());
            }

            Console.WriteLine("RMSEs--------------");
            rmses.ForEach(Console.WriteLine);

            Console.WriteLine("MAEs-------------");
            maes.ForEach(Console.WriteLine);

        }

        public void TestMovieLensWithClusters()
        { 
            // Todo: instead of loading the dataset every time add a updateCluster method to the Dataset<ItemRatingWithCluster>

            List<string> rmses = new List<string>();
            List<string> maes = new List<string>();

            for (int i = 0; i < 15; i += 2)
            {
                string usersClusterFile = Paths.MovieLens1MUsersCluster + i + ".csv";
                string itemsClusterFile = Paths.MovieLens1MItemsCluster + i + ".csv";

                MovieLensReader trainReader, testReader;

                if (i == 0)
                {
                    trainReader = new MovieLensReader(Paths.MovieLens1MTrain75);
                    testReader = new MovieLensReader(Paths.MovieLens1MTest25);
                }
                else
                {
                    trainReader = new MovieLensReader(Paths.MovieLens1MTrain75, usersClusterFile, itemsClusterFile);
                    testReader = new MovieLensReader(Paths.MovieLens1MTest25, usersClusterFile, itemsClusterFile);
                }

                var dataset = new Dataset<MovieLensItemRating>(trainReader, testReader);

                var recommender = new LibFmTrainTester();

                var context = new EvalutationContext<ItemRating>(recommender, dataset);

                var ep = new EvaluationPipeline<ItemRating>(context);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());

                ep.Run();

                rmses.Add(context["RMSE"].ToString());
                maes.Add(context["MAE"].ToString());
            }

            Console.WriteLine("RMSEs--------------");
            rmses.ForEach(Console.WriteLine);

            Console.WriteLine("MAEs-------------");
            maes.ForEach(Console.WriteLine);
        }


        public void TestEpinionClusters()
        {
            List<string> rmses = new List<string>();
            List<string> maes = new List<string>();

            for (int i = 0; i < 15; i += 2)
            {
                string usersClusterFile = Paths.EpinionUsersCluster + i + ".csv";
                string itemsClusterFile = Paths.EpinionItemsCluster + i + ".csv";

                EpinionReader trainReader, testReader;

                if (i == 0)
                {
                    trainReader = new EpinionReader(Paths.EpinionTrain75);
                    testReader = new EpinionReader(Paths.EpinionTest25);
                }
                else
                {
                    trainReader = new EpinionReader(Paths.EpinionTrain75, usersClusterFile, itemsClusterFile);
                    testReader = new EpinionReader(Paths.EpinionTest25, usersClusterFile, itemsClusterFile);
                }

                var dataset = new Dataset<EpinionItemRating>(trainReader, testReader);

                var recommender = new LibFmTrainTester();

                var context = new EvalutationContext<ItemRating>(recommender, dataset);

                var ep = new EvaluationPipeline<ItemRating>(context);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());

                ep.Run();

                rmses.Add(context["RMSE"].ToString());
                maes.Add(context["MAE"].ToString());
            }

            Console.WriteLine("RMSEs--------------");
            rmses.ForEach(Console.WriteLine);

            Console.WriteLine("MAEs-------------");
            maes.ForEach(Console.WriteLine);
        }

        public void Test()
        {
            // step 1: dataset            
            var lines = File.ReadAllLines(Paths.MovieLens1M).Shuffle(1).ToList();


            for (int i = 0; i < 15; i += 2)
            {
                //var dataset = new Dataset<MovieLensItemRating>(new MovieLensReader(Paths.MovieLens1M, Paths.MovieLens1MUsersCluster + i + ".csv", Paths.MovieLens1MItemsCluster + i + ".csv", lines), 0.3);

                // step 2: recommender
                //var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());
                var recommender = new LibFmTrainTester();

                // step3: evaluation
                //var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
                //ep.Evaluators.Add(new RMSE());
                //ep.Evaluators.Add(new MAE());

                //ep.Run();
            }
        }
    }
}
