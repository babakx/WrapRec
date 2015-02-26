using CsvHelper.Configuration;
using MyMediaLite.RatingPrediction;
using System;
using System.Collections.Generic;
using System.IO;
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
        public void Run(int testNum = 3)
        {
            var startTime = DateTime.Now;

            switch (testNum)
            {
                case (1):
                    TestMovieLensSingleDomain();
                    break;
                case(2):
                    TestMovieLensAllDomains(4);
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
                case(7):
                    CreateSubsetEpinions();
                    break;
                default:
                    break;
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("Execution time: {0} sec", (int)duration.TotalSeconds);
        }

        public void TestMovieLensSingleDomain()
        {
            int numDomains = 1;

            // load data
            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            var container = new MovieLensCrossDomainContainer(numDomains);
            movieLensReader.LoadData(container);

            // set taget and active domains
            var targetDomain = container.SpecifyTargetDomain("ml0");
            container.PrintStatistics();

            var startTime = DateTime.Now;

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

            // recommender with non-CrossDomain feature builder
            var model = new MatrixFactorization();
            model.NumIter = 50;
            model.NumFactors = 8;
            model.Regularization = 0.1f;
            //var recommender = new MediaLiteRatingPredictor(model);
            var recommender = new LibFmTrainTester();

            // evaluation
            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());
            ep.Run();
            var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

            Console.WriteLine("RMSE\tDuration\n{0}\t{1}", ctx["RMSE"], duration);

            //container.CreateClusterFiles(Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust.raw", Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust.feat");
            //container.WriteClusters(Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust");
        }

        public void TestAllDomainsAllNumberOfDomains()
        {
            int[] numDomains = new int[] { 15 };

            foreach (int num in numDomains)
            {
                TestMovieLensAllDomains(num);
            }
        }

        public void TestMovieLensAllDomains(int numDomains)
        {
            var numAuxRatings = new List<int> { 1 };

            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            var container = new MovieLensCrossDomainContainer(numDomains, false);
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


        public void TestEpinionAllDomains(int numDomains = 3)
        {
            var numAuxRatings = new List<int> { 0, 1, 2, 3, 4 };

            var epinionsReader = new EpinionsCrossDomainReader(Paths.EpinionRoot + "Epinions RED");
            
            //var domainPaths = Enumerable.Range(1, numDomains)
            //    .Select(i => string.Format("{0}Epinions RED\\Domains{1}-{2}.csv", Paths.EpinionRoot, numDomains, i)).ToArray();
            //var epinionsReader = new EpinionsCrossDomainReader(domainPaths);

            var container = new EpinionsCrossDomainDataContainer(numDomains);
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

        public void CreateSubsetEpinions()
        {
            int numDomains = 4;
            var epinionsReader = new EpinionsCrossDomainReader(Paths.EpinionRoot + "Epinions RED");
            var container = new EpinionsCrossDomainDataContainer(numDomains + 1);
            epinionsReader.LoadData(container);

            var output = container.Users.Values.Where(u =>
            {
                var counts = u.Ratings.Where(r => r.Domain.Id != "ep0").GroupBy(r => r.Domain).Select(g => g.Count());
                return counts.All(c => c >= 1 && c <= 20) && (counts.Count() > 3);
            })
                //.Select(u => new { UserId = u.Id, Counts = u.Ratings.GroupBy(r => r.Domain.Id).Select(g => g.Count().ToString()).Aggregate((a,b) => a + " " + b) })
                //.Select(a => a.UserId + "," + a.Counts);

            .SelectMany(u => u.Ratings)
            .GroupBy(r => r.Domain.Id)
            .Select(g => g.Select(r => r.ToString())).ToList();
                //.SelectMany(u => u.Ratings.GroupBy(r => r.Item.Id).Select(g => g.Take(1).Single()))
            //.Select(r => r.ToString());

            Console.WriteLine("Writing...");
            var header = new string[] { "UserId,ItemId,Rating" };

            int i = 1;
            foreach (var domain in output)
            {
                // Format of the file: Domains{Number of Domains}-{Domain Number}
                File.WriteAllLines(string.Format("{0}Epinions RED\\Domains{1}-{2}.csv", Paths.EpinionRoot, numDomains, i++), header.Concat(domain));
            }
        }
    }
}
