using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Readers;
using WrapRec.Evaluation;
using WrapRec.Entities;
using WrapRec.Recommenders;
using MyMediaLite.RatingPrediction;
using MyMediaLite.ItemRecommendation;
using CsvHelper.Configuration;

namespace WrapRec.Experiments
{
    public class Ectel2014Experiments
    {
        string _maceTraining = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\MACE\mace_training2.csv";
        string _maceTest = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\MACE\mace_test2.csv";

        string _movieLenTrain = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\movielens\movielens_binary_nozero_training.csv";
        string _movieLensTest = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\movielens\movielens_binary_nozero_test.csv";

        string _openScoutTrain = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\open_scout\open_scout_binary_training.csv";
        string _openScoutTest = @"D:\Data\Dropbox\TUDelft joint paper\data\ECTEL 2014\open_scout\open_scout_binary_test.csv";

        public Ectel2014Experiments()
        {

        }
        
        public void Run(int testNum = 1)
        {
            switch (testNum)
            { 
                case(1):
                    TrainAndTest(_openScoutTrain, _openScoutTest);
                    break;
                default:
                    break;
            }
        }

        public void TrainAndTest(string trainSet, string testSet)
        {
            // step 1: dataset            
            var config = new CsvConfiguration();
            config.Delimiter = ";";

            var trainReader = new CsvReader<ItemRanking>(trainSet, config, new ItemRankingMap());
            var testReader = new CsvReader<ItemRanking>(testSet, config, new ItemRankingMap());
            var dataset = new Dataset<ItemRanking>(trainReader, testReader);

            // step 2: recommender
            var algorithm = new SoftMarginRankingMF();
            var recommender = new MediaLiteItemRecommender(algorithm);

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRanking>(new ItemRankingEvaluationContext(recommender, dataset));
            ep.Evaluators.Add(new MediaLiteItemRankingEvaluators(algorithm));
            
            ep.Run();            
        }
    }
}
