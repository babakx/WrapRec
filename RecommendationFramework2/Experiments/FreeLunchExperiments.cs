using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.Readers.NewReaders;
using WrapRec.Recommenders;
using WrapRec.Utilities;

namespace WrapRec.Experiments
{
    public class FreeLunchExperiments
    {
        public void Run(int testNum = 6)
        {
            var startTime = DateTime.Now;

            switch (testNum)
            {
                case (1):
                    TestMovieLens();
                    break;
                case(2):
                    TestMovieLensAllDomains();
                    break;
                case(3):
                    TestAllDomainsAllNumberOfDomains();
                    break;
                case(4):
                    PrepareEpinionDataset();
                    break;
                case(5):
                    ExploreEpinionDataset();
                    break;
                case(6):
                    TestEpinionAllDomains();
                    break;
                default:
                    break;
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("Execution time: {0} sec", (int)duration.TotalSeconds);
        }

        public void TestMovieLens()
        {
            int numDomains = 6;

            // load data
            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            var container = new MovieLensCrossDomainContainer(numDomains);
            movieLensReader.LoadData(container);

            //container.CreateClusterFiles(Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust.raw", Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust.feat");
            //return;

            // set taget and active domains
            var targetDomain = container.SpecifyTargetDomain("ml0");
            //container.DeactiveDomains("ml2", "ml3");

            container.PrintStatistics();

            //container.WriteClusters(Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust");
            //return;

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

            var numAuxRatings = new List<int> { 0, 1, 2, 3 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // recommender
                LibFmTrainTester recommender;
                CrossDomainLibFmFeatureBuilder featureBuilder = null;

                if (num == 0)
                {
                    recommender = new LibFmTrainTester(experimentId: num.ToString());
                }
                else
                {
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(targetDomain, num);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

                // evaluation
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
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAllDomainsAllNumberOfDomains()
        {
            int[] numDomains = new int[] { 2, 4, 6};

            foreach (int num in numDomains)
            {
                TestMovieLensAllDomains(num);
            }
        }

        public void TestMovieLensAllDomains(int numDomains = 4)
        {
            var numAuxRatings = new List<int> { 0, 1, 2, 3 , 4};

            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            var container = new MovieLensCrossDomainContainer(numDomains);
            movieLensReader.LoadData(container);

            double[,] rmseMatrix = new double[numAuxRatings.Count, numDomains];
            int[,] durationsMatrix = new int[numAuxRatings.Count, numDomains];
            int[] numUsers = new int[numDomains];
            int[] numItems = new int[numDomains];
            int[] numRatings = new int[numDomains];

            int domainIndex = 0;

            foreach (Domain d in container.Domains.Values)
            {
                var targetDomain = container.SpecifyTargetDomain(d.Id);
                Console.WriteLine("Target domain: {0}", d.ToString());

                var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

                int numAuxIndex = 0;

                foreach (var num in numAuxRatings)
                {
                    var startTime = DateTime.Now;

                    LibFmTrainTester recommender;
                    CrossDomainLibFmFeatureBuilder featureBuilder = null;

                    if (num == 0)
                    {
                        recommender = new LibFmTrainTester(experimentId: num.ToString());
                    }
                    else
                    {
                        featureBuilder = new CrossDomainLibFmFeatureBuilder(targetDomain, num);
                        recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                    }

                    var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                    var ep = new EvaluationPipeline<ItemRating>(ctx);
                    ep.Evaluators.Add(new RMSE());
                    ep.Run();

                    var duration = DateTime.Now.Subtract(startTime);

                    rmseMatrix[numAuxIndex, domainIndex] = recommender.RMSE;
                    durationsMatrix[numAuxIndex, domainIndex] = (int)duration.TotalMilliseconds;

                    numAuxIndex++;
                }

                numUsers[domainIndex] = d.Ratings.Select(r => r.User.Id).Distinct().Count();
                numItems[domainIndex] = d.Ratings.Select(r => r.Item.Id).Distinct().Count();
                numRatings[domainIndex] = d.Ratings.Count;

                domainIndex++;
            }

            
            // Write RMSEs
            Console.WriteLine("\nRMSEs:\n");

            string header = Enumerable.Range(1, numDomains).Select(i => "D" + i).Aggregate((a, b) => a + "\t" + b);
            Console.WriteLine("Num aux. ratings\t" + header);

            for (int i = 0; i < numAuxRatings.Count; i++)
            {
                Console.Write(numAuxRatings[i]);
                for (int j = 0; j < numDomains; j++)
                {
                    Console.Write("\t" + rmseMatrix[i, j]);
                }
                Console.WriteLine();
            }

            // Write domain statistics
            string users = numUsers.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);
            string items = numItems.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);
            string ratings = numRatings.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);

            Console.WriteLine();
            Console.WriteLine("Num Users\t" + users);
            Console.WriteLine("Num Items\t" + items);
            Console.WriteLine("Num Ratings\t" + ratings);

            // Write times
            Console.WriteLine("\nTimes:\n");

            header = Enumerable.Range(1, numDomains).Select(i => "T" + i).Aggregate((a, b) => a + "\t" + b);
            Console.WriteLine("Num aux. ratings\t" + header);

            for (int i = 0; i < numAuxRatings.Count; i++)
            {
                Console.Write(numAuxRatings[i]);
                for (int j = 0; j < numDomains; j++)
                {
                    Console.Write("\t" + durationsMatrix[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("\n");
        }

        public void PrepareEpinionDataset()
        {
            var ep = new EpinionsSqlFileParser(Paths.EpinionRoot + "epinions_anonym.sql");
            //ep.ParseCategories(Paths.EpinionRoot + "Epinions RED\\Categories.csv");
            //ep.ParseProducts(Paths.EpinionRoot + "Epinions RED\\Products.csv");
            //ep.ParseExpertise(Paths.EpinionRoot + "Epinions RED\\Experties.csv");
            //ep.ParseReviews(Paths.EpinionRoot + "Epinions RED\\Reviews.csv");
            //ep.ParseSimilarity(Paths.EpinionRoot + "Epinions RED\\Similarities.csv");
            ep.ParseTrust(Paths.EpinionRoot + "Epinions RED\\Trusts.csv");
        }

        public void ExploreEpinionDataset()
        {
            var epinionReader = new EpinionsCrossDomainReader(Paths.EpinionRoot + "Epinions RED");
            var container = new EpinionsCrossDomainDataContainer(2);
            epinionReader.LoadData(container);
            container.SpecifyTargetDomain("ep0");

            //container.PrintCategoryStatistics();
            container.PrintStatistics();
        }


        public void TestEpinionAllDomains(int numDomains = 2)
        {
            var numAuxRatings = new List<int> { 0, 1, 2, 3, 4 };

            var epinionsReader = new EpinionsCrossDomainReader(Paths.EpinionRoot + "Epinions RED");
            var container = new EpinionsCrossDomainDataContainer(numDomains + 1);
            epinionsReader.LoadData(container);

            container.Domains.Remove("ep0");

            double[,] rmseMatrix = new double[numAuxRatings.Count, numDomains];
            int[,] durationsMatrix = new int[numAuxRatings.Count, numDomains];
            int[] numUsers = new int[numDomains];
            int[] numItems = new int[numDomains];
            int[] numRatings = new int[numDomains];

            int domainIndex = 0;

            foreach (Domain d in container.Domains.Values)
            {
                var targetDomain = container.SpecifyTargetDomain(d.Id);
                Console.WriteLine("Target domain: {0}", d.ToString());

                var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

                int numAuxIndex = 0;

                foreach (var num in numAuxRatings)
                {
                    var startTime = DateTime.Now;

                    LibFmTrainTester recommender;
                    CrossDomainLibFmFeatureBuilder featureBuilder = null;

                    if (num == 0)
                    {
                        recommender = new LibFmTrainTester(experimentId: num.ToString());
                    }
                    else
                    {
                        featureBuilder = new CrossDomainLibFmFeatureBuilder(targetDomain, num);
                        recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                    }

                    var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                    var ep = new EvaluationPipeline<ItemRating>(ctx);
                    ep.Evaluators.Add(new RMSE());
                    ep.Run();

                    var duration = DateTime.Now.Subtract(startTime);

                    rmseMatrix[numAuxIndex, domainIndex] = recommender.RMSE;
                    durationsMatrix[numAuxIndex, domainIndex] = (int)duration.TotalMilliseconds;

                    numAuxIndex++;
                }

                numUsers[domainIndex] = d.Ratings.Select(r => r.User.Id).Distinct().Count();
                numItems[domainIndex] = d.Ratings.Select(r => r.Item.Id).Distinct().Count();
                numRatings[domainIndex] = d.Ratings.Count;

                domainIndex++;
            }


            // Write RMSEs
            Console.WriteLine("\nRMSEs:\n");

            string header = Enumerable.Range(1, numDomains).Select(i => "D" + i).Aggregate((a, b) => a + "\t" + b);
            Console.WriteLine("Num aux. ratings\t" + header);

            for (int i = 0; i < numAuxRatings.Count; i++)
            {
                Console.Write(numAuxRatings[i]);
                for (int j = 0; j < numDomains; j++)
                {
                    Console.Write("\t" + rmseMatrix[i, j]);
                }
                Console.WriteLine();
            }

            // Write domain statistics
            string users = numUsers.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);
            string items = numItems.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);
            string ratings = numRatings.Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);

            Console.WriteLine();
            Console.WriteLine("Num Users\t" + users);
            Console.WriteLine("Num Items\t" + items);
            Console.WriteLine("Num Ratings\t" + ratings);

            // Write times
            Console.WriteLine("\nTimes:\n");

            header = Enumerable.Range(1, numDomains).Select(i => "T" + i).Aggregate((a, b) => a + "\t" + b);
            Console.WriteLine("Num aux. ratings\t" + header);

            for (int i = 0; i < numAuxRatings.Count; i++)
            {
                Console.Write(numAuxRatings[i]);
                for (int j = 0; j < numDomains; j++)
                {
                    Console.Write("\t" + durationsMatrix[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("\n");
        }
    }
}
