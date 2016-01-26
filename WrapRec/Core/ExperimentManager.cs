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

		StreamWriter _resultWriter;
		string _lastModelId;

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

			var expGroups = Experiments.GroupBy(e => e.Id);

			foreach (var expGroup in expGroups)
			{
				_resultWriter = new StreamWriter(Path.Combine(ResultsFolder, expGroup.Key + ".csv"));
				foreach (Experiment e in expGroup)
				{
					try
					{
						Logger.Current.Info("\nCase {0} of {1}:\n----------------------------------------", caseNo++, numCases);
						LogExperimentInfo(e);
						e.Run();
						LogExperimentResults(e);
						WriteResultsToFile(e);
					}
					catch (Exception ex)
					{
						Logger.Current.Error("Error in expriment {0}, model {1}, split {2}:\n{1}", e.Id, e.Model.Id, e.Split.Id, ex.Message);
					}
				}
				_resultWriter.Close();
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
				Logger.Current.Error("Error in parsing the configuration file: {0}\n", ex.Message);
				Environment.Exit(1);
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

		private void LogExperimentInfo(Experiment exp)
		{
			string format = @"
Experiment Id: {0}
Split Id: {1}
Model Id: {2}
Model Parameteres:
{3}
";
			string modelParameters = exp.Model.AllParameters.Select(kv => kv.Key + ":" + kv.Value)
				.Aggregate((a, b) => a + " " + b);

			Logger.Current.Info(format, exp.Id, exp.Split.Id, exp.Model.Id, modelParameters);
		}

		private void LogExperimentResults(Experiment exp)
		{
			string format = @"\nResults:\n {0}\nTimes:\n Training: {1} Evaluation: {2}\n";

			string results = exp.EvaluationContext.Results.Select(kv => kv.Key + ":" + kv.Value)
				.Aggregate((a, b) => a + " " + b);

			Logger.Current.Info(format, results, exp.TrainTime, exp.EvaluationTime);
		}

		private void WriteResultsToFile(Experiment exp)
		{
			// write a header to the csv file if the model is changed (different models have different parameters)
			if (_lastModelId != exp.Model.Id)
			{
				string header = new string[] { "ExpeimentId", "ModelId", "SplitId" }
					.Concat(exp.Model.AllParameters.Select(kv => kv.Key))
					.Concat(exp.EvaluationContext.Results.Select(kv => kv.Key))
					.Concat(new string[] { "TrainTime", "EvaluationTime", "PureTrainTime", "PureEvaluationTime", "TotalTime", "PureTotalTime" })
					.Aggregate((a, b) => a + ResultSeparator + b);

				_resultWriter.WriteLine(header);
				_lastModelId = exp.Model.Id;
			}

			string result = new string[] { exp.Id, exp.Model.Id, exp.Split.Id }
				.Concat(exp.Model.AllParameters.Select(kv => kv.Value))
				.Concat(exp.EvaluationContext.Results.Select(kv => kv.Value))
				.Concat(new string[] { exp.TrainTime.ToString(), exp.EvaluationTime.ToString(), exp.PureTrainTime.ToString(), exp.PureEvaluationTime.ToString(), 
					(exp.TrainTime + exp.EvaluationTime).ToString(), (exp.PureTrainTime + exp.PureEvaluationTime).ToString() })
				.Aggregate((a, b) => a + ResultSeparator + b);

			_resultWriter.WriteLine(result);
			_resultWriter.Flush();
		}

    }
}
