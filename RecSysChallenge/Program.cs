using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Evaluation;
using WrapRec.Recommenders;
using LibSvm;

namespace RecSysChallenge
{
    class Program
    {
        static string _trainSet = @"D:\Data\Datasets\RecSys Challenge\training.dat";
        static string _testSet = @"D:\Data\Datasets\RecSys Challenge\test.dat";
        
        static void Main(string[] args)
        {
            string trainSet = _trainSet, testSet = _testSet;

            if (args.Length > 0)
            {
                trainSet = args[0];
                testSet = args[1];
            }

            Run(trainSet, testSet);
            Console.WriteLine("Finished!");
            Console.Read();
        }

        static void Run(string trainingSet, string testSet)
        {
            // step 1: dataset            
            var container = new MovieTweetingsDataContainer();

            var reader = new MovieTweetingsReader(trainingSet, testSet);
            reader.LoadData(container);

            Console.WriteLine("Data container statistics:\n {0}", container.ToString());

            var dataset = new ItemRatingDataset(container);

            var featureBuilder = new MovieTweetingLibSvmFeatureBuilder(container);


            // svm parameters
            var svmParameters = new SvmParameter
            {
                SvmType = SvmType.C_SVC,
                KernelType = KernelType.Linear,
                CacheSize = 128,
                C = 1,
                Eps = 1e-3,
                Shrinking = true,
                Probability = false
            };

            // step 2: recommender
            
            var labelSelector = new Func<ItemRating, double>(ir => 
            {
                var t = container.Tweets[ir];
                return ((t.RetweetCount + t.FavoriteCount) > 0) ? 1.0 : 0.0;
            });

            var recommender = new LibSvmClassifier(svmParameters, featureBuilder, labelSelector);

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new WriteChallengeOutput(container, "test_output.dat"));

            ep.Run();
        }
    }
}
