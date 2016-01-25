using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using WrapRec.Models;
using WrapRec.Data;
using WrapRec.Evaluation;
using System.Globalization;
using System.IO;

namespace WrapRec.Core
{
    public class ExperimentManager
    {
        public XElement ConfigRoot { get; private set; }
		public List<Experiment> Experiments { get; private set; }
		public string ResultSeparator { get; private set; }
		public string ResultsFolder { get; set; }

        public ExperimentManager(string configFile)
        {
            ConfigRoot = XDocument.Load(configFile).Root;
			Setup();
        }

		public void RunExperiments()
		{
			int numExperiments = Experiments.GroupBy(e => e.Id).Count();
			int numCases = Experiments.Count;

			Logger.Current.Info("Number of experiments to be done: {0}", numExperiments);
			Logger.Current.Info("Total number of cases: {0}", numCases);

			int caseNo = 1;

			foreach (Experiment e in Experiments)
			{
				e.Run();
			}
		}


		private void Setup()
		{
			try
			{
				XElement allExpEl = ConfigRoot.Descendants("experiments").Single();

				if (allExpEl.Attribute("verbosity") != null && allExpEl.Attribute("verbosity").Value.ToLower() == "trace")
					Logger.Current = NLog.LogManager.GetLogger("traceLogger");

				ResultSeparator = allExpEl.Attribute("separator") != null ? allExpEl.Attribute("separator").Value : ",";
				
				string expFolder = DateTime.Now.ToString("wr yyyy-MM-dd HH.mm", CultureInfo.InvariantCulture);
				string rootPath = allExpEl.Attribute("resultsFolder") != null ? allExpEl.Attribute("resultsFolder").Value 
					: Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				ResultsFolder = Directory.CreateDirectory(Path.Combine(rootPath, expFolder)).FullName;

				XAttribute runAttr = allExpEl.Attribute("run");

				IEnumerable<XElement> expEls = allExpEl.Descendants("experiment");
				if (runAttr != null)
				{
					var expIds = runAttr.Value.Split(',');
					expEls = expEls.Where(el => expIds.Contains(el.Attribute("id").Value));
				}

				Logger.Current.Info("Resolving experiments...");
				Experiments = expEls.SelectMany(el => ParseExperiments(el)).ToList();
			}
			catch (Exception ex)
			{
				Logger.Current.Error(ex.Message);
			}
		}

        private List<Experiment> ParseExperiments(XElement expEl)
        {
			string expId = expEl.Attribute("id").Value;
			string expClass = expEl.Attribute("class") != null ? expEl.Attribute("class").Value : "";

			Type expType;
			if (!string.IsNullOrEmpty(expClass))
			{
				expType = Type.GetType(expClass);
				if (expType == null)
					throw new WrapRecException(string.Format("Can not resolve Experiment type: '{0}'", expClass));

				if (!typeof(Experiment).IsAssignableFrom(expType))
					throw new WrapRecException(string.Format("Experiment type '{0}' should inherit class 'WrapRec.Core.Experiment'", expClass));
			}
			else
				expType = typeof(Experiment);

			// one modelId might map to multiple models (if multiple values are used for parameters)
			IEnumerable<Model> models = expEl.Attribute("models").Value.Split(',')
				.SelectMany(mId => ParseModelsSet(mId));

			// one splitId always map to one split
			IEnumerable<Split> splits = expEl.Attribute("splits").Value.Split(',')
				.Select(sId => ParseSplit(sId));

			var experiments = new List<Experiment>();

			foreach (Model m in models)
			{
				foreach (Split s in splits)
				{
					var exp = (Experiment)expType.GetConstructor(Type.EmptyTypes).Invoke(null);

					exp.Model = m;
					exp.Split = s;
					exp.EvaluationContext = ParseEvaluationContext(expEl.Attribute("evalContext").Value);
					exp.Repeat = expEl.Attribute("repeat") != null ? int.Parse(expEl.Attribute("repeat").Value) : 1;
					exp.Id = expId;

					experiments.Add(exp);
				}
			}

			return experiments;
        }

        private IEnumerable<Model> ParseModelsSet(string modelId)
        {
            return null;
        }

        private Split ParseSplit(string splitId)
        {
            return null;
        }

        private EvaluationContext ParseEvaluationContext(string evalId)
        {
            return null;
        }


    }
}
