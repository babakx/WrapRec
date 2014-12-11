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
        public void Run(int testNum = 1)
        {
            var startTime = DateTime.Now;

            switch (testNum)
            {
                case (1):
                    TestMovieLens();
                    break;
                default:
                    break;
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine("Execution time: {0} sec", (int)duration.TotalSeconds);
        }

        public void TestMovieLens()
        {
            int numDomains = 4;

            // load data
            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            var container = new MovieLensCrossDomainContainer(numDomains);
            movieLensReader.LoadData(container);

            // set taget domain
            var targetDomain = container.Domains["ml0"];
            targetDomain.IsTarget = true;

            container.PrintStatistics();

            //container.WriteClusters(Paths.MovieLens1M.GetDirectoryPath() + "\\movies.clust");
            //return;

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

            var numAuxRatings = new List<int> { 0, 1, 2, 5 };

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
    }
}
