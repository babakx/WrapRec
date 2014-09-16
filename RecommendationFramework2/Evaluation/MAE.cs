using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Evaluation
{
    public class MAE : IEvaluator<ItemRating>
    {

        public void Evaluate(EvalutationContext<ItemRating> context)
        {
            // make sure that the test samples are predicted
            context.RunDefaultTrainAndTest();

            var testset = context.Dataset.TestSamples;

            double sum = 0;
            foreach (var itemRating in testset)
            {
                sum += Math.Abs(itemRating.PredictedRating - itemRating.Rating);
            }

            context["MAE"] = Math.Sqrt(sum / testset.Count());

            Console.WriteLine(string.Format("MAE: {0:0.0000}", context["MAE"]));
        }
    }
}
