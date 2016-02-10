using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;

namespace WrapRec.Evaluation
{
    public class RMSE : Evaluator
    {
		public override void Evaluate(EvaluationContext context, Models.Model model, Data.Split split)
		{
			double sum = 0;
			foreach (var kv in context.PredictedScores)
			{
				sum += Math.Pow(((Rating)kv.Key).Value - kv.Value, 2);
			}

			context.AddResult("rmse", "RMSE", string.Format("{0:0.0000}", Math.Sqrt(sum / context.PredictedScores.Count())));
		}
	}
}
