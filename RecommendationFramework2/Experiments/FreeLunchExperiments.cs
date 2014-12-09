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
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = "::",
                HasHeaderRecord = true
            };

            var movieDomain = new Domain("movie", true);
            var movieLensReader = new CsvReader(Paths.MovieLens1M, config, movieDomain);

            var container = new CrossDomainDataContainer();
            movieLensReader.LoadData(container);

            container.PrintStatistics();
            container.WriteHistogram(Paths.MovieLens1M.GetDirectoryPath());

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

            var numAuxRatings = new List<int> { 0 };

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
                    featureBuilder = new CrossDomainLibFmFeatureBuilder(movieDomain, num);
                    recommender = new LibFmTrainTester(experimentId: num.ToString(), featureBuilder: featureBuilder);
                }

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
        }
    }
}
