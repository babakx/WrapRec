using MyMediaLite;
using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.IO;

namespace WrapRec.Models
{
	public class LibFmWrapper : Model
	{
		public FmFeatureBuilder FeatureBuilder { get; set; }

		public List<string> LibFmArguments { get; private set; }

		float _trainRmse, _testRmse, _lowestTestRmse = float.MaxValue;
		int _currentIter = 1, _lowestIter;
		string _outputPath = "test.out";

		public override void Setup()
		{
			if (SetupParameters.ContainsKey("dim"))
				SetupParameters["dim"] = SetupParameters["dim"].Replace('-', ',');

			if (SetupParameters.ContainsKey("regular"))
				SetupParameters["regular"] = SetupParameters["regular"].Replace('-', ',');

			if (!SetupParameters.ContainsKey("libFmPath"))
				SetupParameters["libFmPath"] = "libfm.net.exe";

			LibFmArguments = SetupParameters.Where(kv => kv.Key.ToLower() != "libfmpath")
				.ToDictionary(kv => "-" + kv.Key, kv => kv.Value)
				.SelectMany(kv => new string[] { kv.Key, kv.Value }).ToList();

			// default data type
			DataType = DataType.Ratings;

			FeatureBuilder = new FmFeatureBuilder();
		}

		protected void UpdateFeatureBuilder(Split split)
		{ 
			if (split.SetupParameters.ContainsKey("userAttributes"))
				foreach (string attr in split.SetupParameters["userAttributes"].Split(','))
					FeatureBuilder.UserAttributes.Add(attr);

			if (split.SetupParameters.ContainsKey("itemAttributes"))
				foreach (string attr in split.SetupParameters["itemAttributes"].Split(','))
					FeatureBuilder.ItemAttributes.Add(attr);

			if (split.SetupParameters.ContainsKey("feedbackAttributes"))
				foreach (string attr in split.SetupParameters["feedbackAttributes"].Split(','))
					FeatureBuilder.FeedbackAttributes.Add(attr);
		}

		public override void Train(Split split)
		{
			UpdateFeatureBuilder(split);
			
			var train = split.Train.Select(f => FeatureBuilder.GetLibFmFeatureVector(f));
			var test = split.Test.Select(f => FeatureBuilder.GetLibFmFeatureVector(f));

			Logger.Current.Info("Creating LibFm train and test files...");

			string trainPath = "train.libfm";
			string testPath = "test.libfm";

			File.WriteAllLines(trainPath, train);
			File.WriteAllLines(testPath, test);

			string args = LibFmArguments.Union(new string[] { "-train", trainPath, "-test", testPath, "-out", _outputPath })
				.Aggregate((a, b) => a + " " + b);

			var libFm = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = SetupParameters["libFmPath"],
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			libFm.OutputDataReceived += OnLibFmOutput;

			Logger.Current.Info("Training and testing with LibFm...");
			Logger.Current.Info("Running process:\n {0} {1}", SetupParameters["libFmPath"], libFm.StartInfo.Arguments);

			PureTrainTime = (int)Wrap.MeasureTime(delegate()
			{
				libFm.Start();
				libFm.BeginOutputReadLine();
				libFm.WaitForExit();
			}).TotalMilliseconds;
		}

		private void OnLibFmOutput(object sender, DataReceivedEventArgs dataLine)
		{
			var data = dataLine.Data;

			if (data != null && (data.StartsWith("Loading") || data.StartsWith("#")))
			{
				Logger.Current.Trace(dataLine.Data);

				if (data.StartsWith("#Iter"))
				{
					int trainIndex = data.IndexOf("Train");
					int testIndex = data.IndexOf("Test");

					_trainRmse = float.Parse(data.Substring(trainIndex + 6, testIndex - trainIndex - 7).TrimEnd(' ', '\t'));
					_testRmse = float.Parse(data.Substring(testIndex + 5).TrimEnd(' ', '\t'));

					if (_testRmse < _lowestTestRmse)
					{
						_lowestTestRmse = _testRmse;
						_lowestIter = _currentIter;
					}

					_currentIter++;
				}
			}
		}

		public override void Evaluate(Split split, EvaluationContext context)
		{
			var results = new Dictionary<string, string>();
			results.Add("LibFmTrainRMSE", string.Format("{0:0.0000}", _trainRmse));
			results.Add("LibFmTestRMSE", string.Format("{0:0.0000}", _testRmse));
			results.Add("LibFmLowestRMSE", string.Format("{0:0.0000}", _lowestTestRmse));
			results.Add("LowestIteration", _lowestIter.ToString());

			context.AddResultsSet("libfm", results);

			if (DataType == DataType.Ratings)
			{
				// test split and tested predictions (in the file) have the same order
				List<float> testPredictions = File.ReadAllLines(_outputPath).Select(l => float.Parse(l)).ToList();
				int i = 0;
				foreach (var feedback in split.Test)
					context.PredictedScores.Add(feedback, testPredictions[i++]);
			}
			
			PureEvaluationTime = (int)Wrap.MeasureTime(delegate()
			{	
				// TODO: make sure that the evaluators that require model.Predict is not used for this model
				context.Evaluators.ForEach(e => e.Evaluate(context, this, split));
			}).TotalMilliseconds;
		}

		public override float Predict(string userId, string itemId)
		{
			throw new NotSupportedException("Rating prediction is not supported with LibFmWrapper class. Use LibFmRecommender instead.");
		}

		public override void Clear()
		{
			FeatureBuilder.RestartNumValues();
			FeatureBuilder.Mapper = new Mapping();
			FeatureBuilder.UserAttributes.Clear();
			FeatureBuilder.ItemAttributes.Clear();
			FeatureBuilder.FeedbackAttributes.Clear();
		}

		public override Dictionary<string, string> GetModelParameters()
		{
			return SetupParameters.Where(kv => kv.Key.ToLower() != "libfmpath").ToDictionary(kv => kv.Key, kv => kv.Value);
		}
	}
}
