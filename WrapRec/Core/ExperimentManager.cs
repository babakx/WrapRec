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
using WrapRec.Utils;

namespace WrapRec.Core
{
    public class ExperimentManager
    {
        public XElement ConfigRoot { get; private set; }
		public IEnumerable<Experiment> Experiments { get; private set; }
		public string ResultSeparator { get; private set; }
		public string ResultsFolder { get; set; }

		StreamWriter _resultWriter;
		string _lastModelId;
		string _lastExpId;

        public ExperimentManager(string configFile)
        {
            ConfigRoot = XDocument.Load(configFile).Root;
			Setup();
        }

		public void RunExperiments()
		{
			int numExperiments = Experiments.Count();
			Logger.Current.Info("Number of experiment cases to be done: {0}", numExperiments);
			int caseNo = 1;

			foreach (Experiment e in Experiments)
			{
				try
				{
					Logger.Current.Info("\nCase {0} of {1}:\n----------------------------------------", caseNo++, numExperiments);
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
			
			if (_resultWriter != null)
				_resultWriter.Close();
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
				Experiments = expEls.SelectMany(el => ParseExperiments(el));
			}
			catch (Exception ex)
			{
				Logger.Current.Error("Error in parsing the configuration file: {0}\n", ex.Message);
				Environment.Exit(1);
			}
		}

        private IEnumerable<Experiment> ParseExperiments(XElement expEl)
        {
			string expId = expEl.Attribute("id").Value;
			string expClass = expEl.Attribute("class") != null ? expEl.Attribute("class").Value : "";

			Type expType;
			if (!string.IsNullOrEmpty(expClass))
			{
				expType = Type.GetType(expClass, true);
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

			foreach (Split s in splits)
			{
				foreach (Model m in models)
				{
                    // if split has subslits (such as cross-validation) for each subsplit a new experiment instance is created
                    if (s.SubSplits != null && s.SubSplits.Count() > 0)
                    {
                        foreach (Split ss in s.SubSplits)
                        {
                            var exp = (Experiment)expType.GetConstructor(Type.EmptyTypes).Invoke(null);

                            exp.Model = m;
                            exp.Split = ss;
                            exp.EvaluationContext = ParseEvaluationContext(expEl.Attribute("evalContext").Value);
                            exp.Id = expId;

							yield return exp;
                        }
                    }
                    else
                    {
                        var exp = (Experiment)expType.GetConstructor(Type.EmptyTypes).Invoke(null);

                        exp.Model = m;
                        exp.Split = s;
                        exp.EvaluationContext = ParseEvaluationContext(expEl.Attribute("evalContext").Value);
                        exp.Id = expId;

						yield return exp;
                    }
				}
			}
        }

        private IEnumerable<Model> ParseModelsSet(string modelId)
        {
            XElement modelEl = ConfigRoot.Descendants("model")
                .Where(el => el.Attribute("id").Value == modelId).Single();

            Type modelType = Type.GetType(modelEl.Attribute("class").Value, true);
            if (!typeof(Model).IsAssignableFrom(modelType))
                throw new WrapRecException(string.Format("Experiment type '{0}' should inherit class 'WrapRec.Models.Model'", modelType));

            var allSetupParams = modelEl.Descendants("parameters").Single()
                .Attributes().ToDictionary(a => a.Name, a => a.Value);

			var paramCartesians = allSetupParams.Select(kv => kv.Value.Split(',').AsEnumerable()).CartesianProduct();

			foreach (IEnumerable<string> pc in paramCartesians)
			{
				var setupParams = allSetupParams.Select(kv => kv.Key)
					.Zip(pc, (k, v) => new { Name = k, Value = v })
					.ToDictionary(kv => kv.Name, kv => kv.Value);

				var model = (Model)modelType.GetConstructor(Type.EmptyTypes).Invoke(null);
				PropertyInfo pi = modelType.GetProperty("SetupParameters");
				pi.SetValue(model, setupParams);

				yield return model;
			}
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
			string modelParameters = exp.Model.GetModelParameters().Select(kv => kv.Key + ":" + kv.Value)
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
			if (_lastExpId != exp.Id)
			{
				if (_resultWriter != null)
					_resultWriter.Close();

				_resultWriter = new StreamWriter(Path.Combine(ResultsFolder, exp.Id + ".csv"));
				_lastExpId = exp.Id;
			}
			
			// write a header to the csv file if the model is changed (different models have different parameters)
			if (_lastModelId != exp.Model.Id)
			{
				string header = new string[] { "ExpeimentId", "ModelId", "SplitId" }
					.Concat(exp.Model.GetModelParameters().Select(kv => kv.Key))
					.Concat(exp.EvaluationContext.Results.Select(kv => kv.Key))
					.Concat(new string[] { "TrainTime", "EvaluationTime", "PureTrainTime", "PureEvaluationTime", "TotalTime", "PureTotalTime" })
					.Aggregate((a, b) => a + ResultSeparator + b);

				_resultWriter.WriteLine(header);
				_lastModelId = exp.Model.Id;
			}

			string result = new string[] { exp.Id, exp.Model.Id, exp.Split.Id }
				.Concat(exp.Model.GetModelParameters().Select(kv => kv.Value))
				.Concat(exp.EvaluationContext.Results.Select(kv => kv.Value))
				.Concat(new string[] { exp.TrainTime.ToString(), exp.EvaluationTime.ToString(), exp.Model.PureTrainTime.ToString(), exp.Model.PureEvaluationTime.ToString(), 
					(exp.TrainTime + exp.EvaluationTime).ToString(), (exp.Model.PureTrainTime + exp.Model.PureEvaluationTime).ToString() })
				.Aggregate((a, b) => a + ResultSeparator + b);

			_resultWriter.WriteLine(result);
			_resultWriter.Flush();
		}

    }
}
