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
using LinqLib.Sequence;
using WrapRec.Core;

namespace WrapRec.Models
{
	public class LibFmWrapper : Model
	{
		public FmFeatureBuilder FeatureBuilder { get; set; }

		public List<string> LibFmArguments { get; private set; }

        float _trainRmse, _testRmse, _lowestTestRmse = float.MaxValue;
		int _currentIter = 1, _lowestIter;
		string _outputPath = "test.out";

        float _minTarget, _maxTarget;

        List<float> _w;
        List<float[]> _v;

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

			if (!SetupParameters.ContainsKey("save_model"))
				LibFmArguments.Add("-save_model train.model");

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
			
			Logger.Current.Info("Creating LibFm train and test files...");


			// infer dataType and add negative samples in case of posFeedback data
			IEnumerable<Feedback> negFeedback = Enumerable.Empty<Feedback>();
			if (split.Container.DataReaders.Select(dr => dr.DataType).Contains(IO.DataType.PosFeedback))
			{
				this.DataType = IO.DataType.PosFeedback;
				negFeedback = split.SampleNegativeFeedback((int)(split.Train.Count()));
			}

			var train = split.Train.Concat(negFeedback).Shuffle().Select(f => FeatureBuilder.GetLibFmFeatureVector(f));
			var test = split.Test.Select(f => FeatureBuilder.GetLibFmFeatureVector(f));

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

            LoadFmModel();
            _minTarget = split.Container.MinTarget;
            _maxTarget = split.Container.MaxTarget;
		}

        private void LoadFmModel()
        {
            string modelFile = SetupParameters.ContainsKey("save_model") ?
                SetupParameters["save_model"] : "train.model";

            modelFile = modelFile.Replace("\"", "");

            var lines = File.ReadAllLines(modelFile);

			// TODO: if dim is 0,0,x w0 and w would be 0 and the format of the file would be different
            float w0 = float.Parse(lines.Skip(1).First());
            _w = new float[] { w0 }.Concat(
                lines.Skip(3).TakeWhile(l => !l.StartsWith("#")).Select(l => float.Parse(l))).ToList();

            _v = lines.Skip(_w.Count + 3)
                .Select(l => l.Split(' ').Select(v => float.Parse(v)).ToArray()).ToList();
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

                    // TODO: iteratoin time should be measured
                    //OnIterate(this, 0);
				}
			}
		}

		public override void Evaluate(Split split, EvaluationContext context)
		{
			if (DataType == DataType.Ratings)
			{
				var results = new Dictionary<string, string>();
				results.Add("LibFmTrainRMSE", string.Format("{0:0.0000}", _trainRmse));
				results.Add("LibFmTestRMSE", string.Format("{0:0.0000}", _testRmse));
				results.Add("LibFmLowestRMSE", string.Format("{0:0.0000}", _lowestTestRmse));
				results.Add("LowestIteration", _lowestIter.ToString());

				context.AddResultsSet("libfm", results);
	
				// test split and tested predictions (in the file) have the same order
				List<float> testPredictions = File.ReadAllLines(_outputPath).Select(l => float.Parse(l)).ToList();
				int i = 0;
				foreach (var feedback in split.Test)
					// no need to call Predict(feedback) becuase the predictions are already saved in the output file
					context.PredictedScores.Add(feedback, testPredictions[i++]);
            }
			
			PureEvaluationTime = (int)Wrap.MeasureTime(delegate()
			{	
				context.Evaluators.ForEach(e => e.Evaluate(context, this, split));
			}).TotalMilliseconds;
		}

		public override float Predict(string userId, string itemId)
		{
            return Predict(FeatureBuilder.GetLibFmFeatureVector(userId, itemId));
        }

        public override float Predict(Feedback feedback)
        {
            return Predict(FeatureBuilder.GetLibFmFeatureVector(feedback));
        }

        public virtual float Predict(string featureVector)
        {
            var featIds = new List<int>();
            var featValues = new List<float>();

            foreach (string feat in featureVector.Split(' '))
            {
                var parts = feat.Split(':');

                // if feedback is rating the first part of split is rating (format is 5 0:1 1:1)
                if (parts.Count() < 2)
                    continue;

                featIds.Add(int.Parse(parts[0]));
                featValues.Add(float.Parse(parts[1]));
            }

            int k = _v[0].Count();
            float p = _w[0];

            // Check LibFM paper eq. (5)
            for (int f = 0; f < k; f++)
            {
				float sum = 0, sumSqr = 0;
				for (int i = 0; i < featIds.Count; i++)
                {
                    if (featIds[i] < _v.Count)
                    {
                        float t = _v[featIds[i]][f] * featValues[i];
                        sum += t;
                        sumSqr += t * t;

                        if (f == 0)
                            p += _w[featIds[i] + 1] * featValues[i];
                    }
				}
				p += 0.5f * (sum * sum - sumSqr);
			}

			if (DataType == IO.DataType.Ratings)
			{
				p = Math.Min(p, _maxTarget);
				p = Math.Max(p, _minTarget);
			}

            return p;
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
