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
    public class RecSys2015Experiments
    {
        public void Run(int testNum = 1)
        {
            switch (testNum)
            {
                case (1):
                    TestMovieLensSingle();
                    break;
                default:
                    break;
            }
        }

        public void TestMovieLensSingle()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = "::",
                HasHeaderRecord = true
            };

            // load data
            var trainReader = new CsvReader(Paths.MovieLens1MTrain75, config);
            var testReader = new CsvReader(Paths.MovieLens1MTest25, config, true);

            var container = new DataContainer();
            trainReader.LoadData(container);
            testReader.LoadData(container);

            var startTime = DateTime.Now;

            var splitter = new RatingSimpleSplitter(container);

            //var recommender = new MediaLiteRatingPredictor(new MatrixFactorization());
            var recommender = new LibFmTrainTester(libFmPath: "LibFm.Net.64.exe");

            // evaluation
            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Run();

            var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

            Console.WriteLine("RMSE\tDuration\n{0}\t{1}", ctx["RMSE"], duration);
        }
    }
}
