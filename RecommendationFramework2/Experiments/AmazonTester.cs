using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RF2.Readers;
using RF2.Evaluation;
using RF2.Entities;
using RF2.Recommenders;
using RF2.Utilities;
using MyMediaLite.RatingPrediction;
using System.IO;

namespace RF2.Experiments
{
    public class AmazonTester
    {
        string _booksPath = @"D:\Data\Datasets\Amazon\Old\ECIR 2014 Dataset\books_selected.csv";
        string _trainPath = @"D:\Data\Datasets\Amazon\Old\ECIR 2014 Dataset\books_selected_train.csv";
        string _testPath = @"D:\Data\Datasets\Amazon\Old\ECIR 2014 Dataset\books_selected_test.csv";

        public AmazonTester()
        { 
        
        }

        public void Run(int testNum = 1)
        {
            switch (testNum)
            {
                case (1):
                    TestBooksWithMF();
                    break;
                default:
                    break;
            }
        }

        public void TestBooksWithMF()
        {
            // step 1: dataset            
            if (!File.Exists(_trainPath))
                FileHelper.SplitLines(_booksPath, _trainPath, _testPath, 0.75, true, true);

            var trainReader = new CsvReader<ItemRating>(_trainPath, new ItemRatingMap());
            var testReader = new CsvReader<ItemRating>(_testPath, new ItemRatingMap());
            
            var dataset = new Dataset<ItemRating>(trainReader, testReader);

            // step 2: recommender
            var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            
            ep.Run();
        }
    }
}
