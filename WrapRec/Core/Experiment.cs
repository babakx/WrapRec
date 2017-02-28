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
	    public int CurrentIter { get; private set; }
        public int MultiEval { get; private set; }
        public int EpochTime { get; private set; }

	    public virtual void Setup()
	    {
	        if (!Split.Container.IsLoaded)
	        {
	            Logger.Current.Info("Loading DataContainer '{0}'...", Split.Container.Id);
	            Split.Container.Load();
	        }
	        if (!EvaluationContext.IsSetuped)
	        {
	            Logger.Current.Info("Setuping evaluation context '{0}'...", EvaluationContext.Id);
	            EvaluationContext.Setup();
	        }

	        Logger.Current.Info("Setuping model '{0}'...", Model.Id);
	        Model.Setup();

	        if (SetupParameters.ContainsKey("multiEval"))
	        {
	            MultiEval = int.Parse(SetupParameters["multiEval"]);
                Model.Iterated += ModelIterated;
            }
	    }

	    private void ModelIterated(object sender, int epochTime)
	    {
            CurrentIter++;
            if (CurrentIter%MultiEval == 0)
            {
                Logger.Current.Info("Evaluating on iteration {0}. Iteration time: {1} milliseconds.", CurrentIter, epochTime);
                Model.Evaluate(Split, EvaluationContext);
                EpochTime = epochTime;
	            ExperimentManager.WriteResultsToFile(this);
                EvaluationContext.Clear();
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
