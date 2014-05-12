using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RF2.Data;

namespace RF2.Evaluation
{
    public class RMSE : IEvaluator<ItemRating>
    {
        public void Evaluate(EvalutationContext<ItemRating> context)
        {
            // make sure that the test samples are predicted
            context.RunDefaultTrainAndTest();

            var testset = context.Dataset.TestSamples;

            double sum = 0;
            foreach (var itemRating in testset)
            {
                sum += Math.Pow(itemRating.PredictedRating - itemRating.Rating, 2);
            }

            context["RMSE"] = Math.Sqrt(sum / testset.Count());

            Console.WriteLine(string.Format("RMSE: {0:0.0000}", context["RMSE"]));
        }
    }
}
