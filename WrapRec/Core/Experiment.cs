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
		public string Id { get; set; }
		public Model Model { get; set; }
		public Split Split { get; set; }
		public EvaluationContext EvaluationContext { get; set; }
		public int TrainTime { get; private set; }
		public int EvaluationTime { get; private set; }
		public int PureTrainTime { get; private set; }
		public int PureEvaluationTime { get; private set; }

		public void Run()
		{
			TrainTime = (int)Wrap.MeasureTime(delegate() { Model.Train(Split); }).TotalMilliseconds;
			EvaluationTime = (int)Wrap.MeasureTime(delegate() { Model.Evaluate(Split, EvaluationContext); }).TotalMilliseconds;
			PureTrainTime = Model.GetPureTrainTime();
			PureEvaluationTime = Model.GetPureEvaluationTime();
		}
	}
}
