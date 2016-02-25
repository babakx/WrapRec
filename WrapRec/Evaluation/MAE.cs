using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Models;

namespace WrapRec.Evaluation
{
    public class MAE : Evaluator
    {
		public override void Evaluate(EvaluationContext context, Model model, Split split)
		{
			double sum = 0;
			foreach (var kv in context.PredictedScores)
			{
				sum += Math.Abs(((Rating)kv.Key).Value - kv.Value);
			}

			context.AddResult("mae", "MAE", string.Format("{0:0.0000}", sum / context.PredictedScores.Count()));
		}
	}
}
