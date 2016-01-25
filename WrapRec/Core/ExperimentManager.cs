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
		public List<ExperimentBundle> ExperimentBundles { get; private set; }
		public string ResultSeparator { get; private set; }
		public string ResultsFolder { get; set; }

        public ExperimentManager(string configFile)
        {
            ConfigRoot = XDocument.Load(configFile).Root;
			Setup();
        }

		public void RunExperiments()
		{
			int numExperiments = ExperimentBundles.Count;
			int numCases = ExperimentBundles.Sum(eb => eb.Experiments.Count);

			Logger.Current.Info("Number of experiments to be done: {0}", numExperiments);
			Logger.Current.Info("Total number of cases: {0}", numCases);

			int caseNo = 1;

			foreach (ExperimentBundle e in ExperimentBundles)
			{

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
				ExperimentBundles = expEls.Select(el => ParseExperiment(el)).ToList();
			}
			catch (Exception ex)
			{
				Logger.Current.Error(ex.Message);
			}
		}

        private ExperimentBundle ParseExperiment(XElement expEl)
        {
            string expClass = expEl.Attribute("class") != null ? expEl.Attribute("class").Value : "";
            var bundle = new ExperimentBundle(expClass);

            bundle.Id = expEl.Attribute("id").Value;

            expEl.Attribute("models").Value.Split(',').ToList()
				.ForEach(mId => bundle.Models.AddRange(ParseModelsSet(mId)));

            expEl.Attribute("splits").Value.Split(',').ToList()
                .ForEach(sId => bundle.Splits.AddRange(ParseSplitsSet(sId)));

            bundle.EvaluationContext = ParseEvaluationContext(expEl.Attribute("evalContext").Value);
            bundle.Repeat = expEl.Attribute("repeat") != null ? int.Parse(expEl.Attribute("repeat").Value) : 1;

            return bundle;
        }

        private IEnumerable<Model> ParseModelsSet(string modelId)
        {
            return null;
        }

        private IEnumerable<Split> ParseSplitsSet(string splitId)
        {
            return null;
        }

        private EvaluationContext ParseEvaluationContext(string evalId)
        {
            return null;
        }


    }
}
