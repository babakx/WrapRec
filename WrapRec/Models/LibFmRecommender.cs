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
	public class LibFmRecommender : LibFmWrapper
	{
		// this is reference to managed wrapper of libFm C++ code
		LibFmManager _libFm;
		
		public override void Setup()
		{
			base.Setup();

			_libFm = new LibFmManager();
			_libFm.Setup(LibFmArguments);
		}

		public override void Train(Split split)
		{
			UpdateFeatureBuilder(split);

			List<string> train = split.Train.Select(f => FeatureBuilder.GetLibFmFeatureVector(f)).ToList();

			_libFm.CreateTrainSet(train, split.Container.MinTarget, split.Container.MaxTarget,
				FeatureBuilder.GetNumMappedValues(), FeatureBuilder.Mapper.NumberOfEntities);

			Logger.Current.Trace("Training with LibFm recommender...");
			PureTrainTime = (int)Wrap.MeasureTime(delegate() 
			{ 
				_libFm.Train(); 
			}).TotalMilliseconds;
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
			return Predict(FeatureBuilder.GetLibFmFeatureVector(userId, itemId));
		}

		public override float Predict(Feedback feedback)
		{
			return Predict(FeatureBuilder.GetLibFmFeatureVector(feedback));
		}

		public override float Predict(string featureVector)
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
			base.Clear();
		}

	}
}
