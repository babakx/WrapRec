using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Models;
using WrapRec.Evaluation;
using WrapRec.Data;
using WrapRec.Utilities;

namespace WrapRec.Core
{
    public class Experiment : IExperiment
    {
        public List<Model> Models { get; private set; }
        public List<ISplit> Splits { get; private set; }
        public EvaluationContext EvaluationContext { get; set; }
        public string Id { get; set; }
        public int Repeat { get; set; }
		public int NumCases { get; private set; }
		public List<ExperimentResult> Results { get; private set; }

        public Experiment()
        {
            Models = new List<Model>();
            Splits = new List<ISplit>();
			Results = new List<ExperimentResult>();
        }

		
		public void Run(int repeatNum = 1)
        {
			foreach (Model m in Models)
			{
				foreach (ISplit s in Splits)
				{
					var er = new ExperimentResult() 
					{
 						RepeatNum = repeatNum,
						Model = m,
						Split = s,
						EvaluationContext = EvaluationContext
					};

					er.TrainTime = (int)Wrap.MeasureTime(delegate() { m.Train(s); }).TotalMilliseconds;
					er.EvaluationTime = (int)Wrap.MeasureTime(delegate() { m.Evaluate(s, EvaluationContext); }).TotalMilliseconds;
					er.PureTrainTime = m.GetPureTrainTime();
					er.PureEvaluationTime = m.GetPureEvaluationTime();
				}
			}
        }
    }
}
