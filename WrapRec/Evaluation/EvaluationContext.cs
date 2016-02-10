using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Utils;

namespace WrapRec.Evaluation
{
    public class EvaluationContext
    {
		public string Id { get; set; }
        public List<Evaluator> Evaluators { get; private set; }
        public Dictionary<Feedback, float> PredictedScores { get; private set; }
		public bool IsSetuped { get; private set; }

		Dictionary<string, List<Dictionary<string, string>>> _results;

        public EvaluationContext()
        {
            Evaluators = new List<Evaluator>();
            PredictedScores = new Dictionary<Feedback, float>();
			_results = new Dictionary<string, List<Dictionary<string, string>>>();
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
			_results.Clear();
			PredictedScores.Clear();
        }

		/// <summary>
		/// This method should be used if the evaluator generates only one single result
		/// </summary>
		/// <param name="evaluatorName"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
 		public void AddResult(string evaluatorName, string key, string value)
		{
			var resultDic = new Dictionary<string, string>();
			resultDic.Add(key, value);
			AddResultsSet(evaluatorName, resultDic);
		}

		/// <summary>
		/// This method should be used only if the evaluator generates more than one results depending on input parameters 
		/// each resultsSet together with the parameteres that leaded to those results should be represented by a dictionary
		/// <param name="evaluatorName"></param>
		/// <param name="results"></param>
		public void AddResultsSet(string evaluatorName, Dictionary<string, string> results)
		{
			if (_results.ContainsKey(evaluatorName))
				_results[evaluatorName].Add(results);
			else
			{
				var dicSet = new List<Dictionary<string, string>>();
				dicSet.Add(results);
				_results[evaluatorName] = dicSet;
			}
		}

		public IEnumerable<Dictionary<string, string>> GetResults()
		{
			return _results.Values.CartesianProduct()  // here we have cartesian products, each containing a set of dictionaries
				.Select(dicSet => dicSet.SelectMany(d => d).ToDictionary(kv => kv.Key, kv => kv.Value));
		}
    }
}
