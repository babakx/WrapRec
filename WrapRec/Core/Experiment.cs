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
	public enum ExperimentType
	{
 		Evaluation,
		Other
	}

	public class Experiment
	{
		public string Id { get; set; }
		public Model Model { get; set; }
		public Split Split { get; set; }
		public EvaluationContext EvaluationContext { get; set; }
		public int TrainTime { get; private set; }
		public int EvaluationTime { get; private set; }
		public ExperimentManager ExperimentManager { get; set; }
		public Dictionary<string, string> SetupParameters { get; set; }
		public ExperimentType Type { get; set; }

		public virtual void Setup()
		{
			if (!Split.Container.IsLoaded)
			{
				Logger.Current.Info("Loading DataContainer '{0}'...", Split.Container.Id);
				Split.Container.Load();
			}
			Logger.Current.Info("Setuping model '{0}'...", Model.Id);
			Model.Setup();
			if (!EvaluationContext.IsSetuped)
			{
				Logger.Current.Info("Setuping evaluation context '{0}'...", EvaluationContext.Id);
				EvaluationContext.Setup();
			}
		}
		
		public virtual void Run()
		{
			Logger.Current.Info("Training...");
            TrainTime = (int)Wrap.MeasureTime(delegate() { Model.Train(Split); }).TotalMilliseconds;
            Logger.Current.Info("Evaluating...");
            EvaluationTime = (int)Wrap.MeasureTime(delegate() { Model.Evaluate(Split, EvaluationContext); }).TotalMilliseconds;
		}

        public virtual void Clear()
        {
			Model.Clear();
            EvaluationContext.Clear();
        }
	}
}
