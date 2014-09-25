using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Evaluation;
using System.IO;

namespace RecSysChallenge
{
    public class WriteChallengeOutput : IEvaluator<ItemRating>
    {
        MovieTweetingsDataContainer _container;
        string _outputFile;
        
        public WriteChallengeOutput(MovieTweetingsDataContainer container, string outputPath)
        {
            _container = container;
            _outputFile = outputPath;
        }
        
        public void Evaluate(EvalutationContext<ItemRating> context)
        {
            // make sure that the test samples are predicted
            context.RunDefaultTrainAndTest();

            var output = context.Dataset.TestSamples.AsEnumerable()
                .Select(ir =>
                {
                    var t = _container.Tweets[ir];

                    return string.Format("{0},{1},{2}", t.TwitterUserId, t.Id, ir.PredictedRating);
                });


            File.WriteAllLines(_outputFile, output);
        }
    }
}
