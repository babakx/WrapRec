using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;

namespace WrapRec.Models
{
	public abstract class Model : IModel
	{
		public string Id { get; set; }
		public abstract void Setup(Dictionary<string, string> modelParams);
		public abstract void Train(ISplit split);
		public abstract void Evaluate(ISplit split, EvaluationContext context);

		public int GetPureTrainTime()
		{
			return -1;
		}

		public int GetPureEvaluationTime()
		{
			return -1;
		}
	}
}
