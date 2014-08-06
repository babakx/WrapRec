using CsvHelper.Configuration;
using MyMediaLite.RatingPrediction;
using RF2.Evaluation;
using RF2.Readers;
using RF2.Recommenders;
using RF2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Experiments
{
    public class Journal2014Experiments
    {

        public void Run(int testNum = 1)
        {
            switch (testNum)
            { 
                case(1):
                    TestAmazonDatasetSingle();
                    break;
                default:
                    break;
            }
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
            var recommender = new LibFmTrainTester();

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();            
        }
    }
}
