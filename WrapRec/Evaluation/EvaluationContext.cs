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
		public string Id { get; set; }
		public Dictionary<string, string> Results { get; private set; }
        public List<Evaluator> Evaluators { get; private set; }
        public Dictionary<Feedback, float> PredictedScores { get; private set; }
		public bool IsSetuped { get; private set; }

        public EvaluationContext()
        {
            Evaluators = new List<Evaluator>();
            Results = new Dictionary<string, string>();
            PredictedScores = new Dictionary<Feedback, float>();
        }

		public void Setup()
		{
			Evaluators.ForEach(e => e.Setup());
			IsSetuped = true;
		}

        public void AddEvaluator(Evaluator evaluator)
        {
            Evaluators.Add(evaluator);
        }

        public void Clear()
        {
            Results.Clear();
        }
    }
}
