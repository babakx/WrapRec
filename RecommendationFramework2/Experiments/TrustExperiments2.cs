using CsvHelper.Configuration;
using MyMediaLite.RatingPrediction;
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
    public class TrustExperiments2
    {
        public void Run(int testNum = 2)
        {
            switch (testNum)
            {
                case (1):
                    TestEpinionsSingle();
                    break;
                case(2):
                    TestEpinionsTrustAware();
                    break;
                default:
                    break;
            }
        }

        public void TestEpinionsSingle()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = " ",
                HasHeaderRecord = true
            };

            // load data
            var trainReader = new CsvReader(Paths.EpinionTrain80, config);
            var testReader = new CsvReader(Paths.EpinionTest20, config, true);
            
            var container = new DataContainer();
            trainReader.LoadData(container);
            testReader.LoadData(container);

            var startTime = DateTime.Now;

            var splitter = new RatingSimpleSplitter(container);

            //var recommender = new MediaLiteRatingPredictor(new MatrixFactorization());
            var recommender = new LibFmTrainTester();

            // evaluation
            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Run();

            var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

            Console.WriteLine("RMSE\tDuration\n{0}\t{1}", ctx["RMSE"], duration);
        }

        public void TestEpinionsTrustAware()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = " ",
                HasHeaderRecord = true
            };

            // load data
            var trainReader = new CsvReader(Paths.EpinionTrain80, config);
            var testReader = new CsvReader(Paths.EpinionTest20, config, true);
            var readers = new List<IDatasetReader>() { trainReader, testReader };
            var epinionTrustReader = new EpinionTrustReader(readers.ToArray(), Paths.EpinionRelationsImplicit);

            var container = new DataContainer();
            epinionTrustReader.LoadData(container);

            var startTime = DateTime.Now;

            var splitter = new RatingSimpleSplitter(container);

            //var recommender = new MediaLiteRatingPredictor(new MatrixFactorization());
            var fb = new TrustAwareLibFmFeatureBuilder(container, 4, true);
            var recommender = new LibFmTrainTester(featureBuilder: fb);
            

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
