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
    public class ExperimentBundle 
    {
        public List<Model> Models { get; private set; }
        public List<Split> Splits { get; private set; }
        public EvaluationContext EvaluationContext { get; set; }
        public string Id { get; set; }
        public int Repeat { get; set; }
		public List<Experiment> Experiments { get; private set; }
		public string ExperimentClass { get; set; }


        public ExperimentBundle(string expClass)
        {
            Models = new List<Model>();
            Splits = new List<Split>();
			Experiments = new List<Experiment>();
			ExperimentClass = expClass;
			SetupExperiments();
        }
		
		private void SetupExperiments()
        {
			Type expType;

			if (!string.IsNullOrEmpty(ExperimentClass))
			{
				expType = Type.GetType(ExperimentClass);
				if (expType == null)
					throw new WrapRecException(string.Format("Can not resolve Experiment type: '{0}'", ExperimentClass));

				if (!typeof(Experiment).IsAssignableFrom(expType))
					throw new WrapRecException(string.Format("Experiment type '{0}' should inherit class 'WrapRec.Core.Experiment'", ExperimentClass));
			}
			else
				expType = typeof(Experiment);
			
			
			foreach (Model m in Models)
			{
				foreach (Split s in Splits)
				{
					var exp = (Experiment)expType.GetConstructor(Type.EmptyTypes).Invoke(null);

					exp.Model = m;
					exp.Split = s;
					exp.EvaluationContext = EvaluationContext;

					Experiments.Add(exp);
				}
			}
        }
    }
}
