using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Models;
using WrapRec.Evaluation;
using WrapRec.Data;
using MyMediaLite;

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

		public void Run()
		{
            Logger.Current.Info("Training...");
            TrainTime = (int)Wrap.MeasureTime(delegate() { Model.Train(Split); }).TotalMilliseconds;
            Logger.Current.Info("Evaluating...");
            EvaluationTime = (int)Wrap.MeasureTime(delegate() { Model.Evaluate(Split, EvaluationContext); }).TotalMilliseconds;
		}

        public void Clear()
        {
            Model.Clear();
            EvaluationContext.Clear();
            Split.Clear();
        }
	}
}
