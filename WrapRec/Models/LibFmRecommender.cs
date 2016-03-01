using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;
using LibFm;
using MyMediaLite;
using WrapRec.IO;
using WrapRec.Core;
using MyMediaLite.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace WrapRec.Models
{
	public class LibFmRecommender : Model
	{
		// this is reference to managed wrapper of libFm C++ code
		LibFmManager _libFm;
		FmFeatureBuilder _featureBuilder;
		
		public override void Setup()
		{
			if (SetupParameters.ContainsKey("dim"))
				SetupParameters["dim"] = SetupParameters["dim"].Replace('-', ',');

			if (SetupParameters.ContainsKey("regular"))
				SetupParameters["regular"] = SetupParameters["regular"].Replace('-', ',');

			// default data type
			DataType = DataType.Ratings;

			List<string> allParams = SetupParameters.ToDictionary(kv => "-" + kv.Key, kv => kv.Value)
				.SelectMany(kv => new string[] { kv.Key, kv.Value }).ToList();
			_libFm = new LibFmManager();
			_libFm.Setup(allParams);

			_featureBuilder = new FmFeatureBuilder();
		}

		public override void Train(Split split)
		{
			List<string> train = split.Train.Select(f => _featureBuilder.GetLibFmFeatureVector(f)).ToList();

			_libFm.CreateTrainSet(train, split.Container.MinTarget, split.Container.MaxTarget,
				_featureBuilder.GetNumMappedValues(), _featureBuilder.Mapper.NumberOfEntities);

			Logger.Current.Trace("Training with LibFm recommender...");
			PureTrainTime = (int)Wrap.MeasureTime(delegate() { 
				_libFm.Train(); })
				.TotalMilliseconds;
		}

		public override void Evaluate(Split split, EvaluationContext context)
		{
			PureEvaluationTime = (int)Wrap.MeasureTime(delegate()
			{
				if (DataType == DataType.Ratings)
					foreach (var feedback in split.Test)
						context.PredictedScores.Add(feedback, Predict(feedback));

				context.Evaluators.ForEach(e => e.Evaluate(context, this, split));
			}).TotalMilliseconds;
		}

		public override float Predict(string userId, string itemId)
		{
			return Predict(_featureBuilder.GetLibFmFeatureVector(userId, itemId));
		}

		public float Predict(Feedback feedback)
		{
			return Predict(_featureBuilder.GetLibFmFeatureVector(feedback));
		}

		public float Predict(string featureVector)
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

			var featVector = new FeatureVector();
			featVector.featureIds = featIds;
			featVector.featureValues = featValues;

			return (float)_libFm.Predict(featVector);
		}

		public override void Clear()
		{
			_libFm.Clear();
			_featureBuilder.RestartNumValues();
			_featureBuilder.Mapper = new Mapping();
		}

		public override Dictionary<string, string> GetModelParameters()
		{
			return SetupParameters;
		}
	}
}
