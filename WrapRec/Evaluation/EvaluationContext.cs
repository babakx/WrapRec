using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;

namespace WrapRec.Evaluation
{
    public class EvaluationContext
    {
		public Dictionary<string, string> Results { get; private set; }
        public List<IEvaluator> Evaluators { get; private set; }
        public Dictionary<Feedback, float> PredictedScores { get; private set; }

        public EvaluationContext()
        {
            Evaluators = new List<IEvaluator>();
            Results = new Dictionary<string, string>();
            PredictedScores = new Dictionary<Feedback, float>();
        }

        public void AddEvaluator(IEvaluator evaluator)
        {
            Evaluators.Add(evaluator);
        }

        public void Clear()
        {
            Results.Clear();
        }
    }
}
