using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Evaluation;
using WrapRec.Data;
using System.Reflection;
using MyMediaLite;
using WrapRec.Utils;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using WrapRec.IO;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;

namespace WrapRec.Models
{
	public class MmlRecommender : Model
	{
		Mapping _usersMap;
		Mapping _itemsMap;
		Type _mmlRecommenderType;
		IRecommender _mmlRecommenderInstance;

		public DataType DataType { get; private set; }

		public MmlRecommender()
        {
            _usersMap = new Mapping();
            _itemsMap = new Mapping();
        }

		public override void Setup()
		{
			try
			{
				// build recommender object
				_mmlRecommenderType = Helpers.ResolveType(SetupParameters["ml-class"]);
				_mmlRecommenderInstance = (IRecommender)_mmlRecommenderType.GetConstructor(Type.EmptyTypes).Invoke(null);

				if (typeof(IRatingPredictor).IsAssignableFrom(_mmlRecommenderType))
					DataType = DataType.Ratings;
				else if (typeof(ItemRecommender).IsAssignableFrom(_mmlRecommenderType))
					DataType = DataType.PosFeedback;
				else
					throw new WrapRecException(string.Format("Unknown MmlRecommender class: {0}", SetupParameters["ml-class"]));

				// Set properties
				foreach (var param in SetupParameters.Where(kv => kv.Key != "ml-class"))
				{
					PropertyInfo pi = _mmlRecommenderInstance.GetType().GetProperty(param.Key);
					pi.SetValue(_mmlRecommenderInstance, Convert.ChangeType(param.Value, pi.PropertyType));
				}
			}
			catch (Exception ex)
			{
				throw new WrapRecException(string.Format("Cannot resolve MmlRecommender: {0}\n{1}", ex.Message, ex.StackTrace));
			}
		}

		public override void Train(Split split)
		{
			// Convert trainset to MyMediaLite trianset format
			if (DataType == IO.DataType.Ratings)
			{
				var mmlFeedback = new Ratings();
				foreach (var feedback in split.Train)
				{
					var rating = (Rating)feedback;
					mmlFeedback.Add(_usersMap.ToInternalID(rating.User.Id), _itemsMap.ToInternalID(rating.Item.Id), rating.Value);
				}
				((IRatingPredictor)_mmlRecommenderInstance).Ratings = mmlFeedback;
			}
			else
			{
				var mmlFeedback = new PosOnlyFeedback<SparseBooleanMatrix>();
				foreach (var feedback in split.Train)
				{
					mmlFeedback.Add(_usersMap.ToInternalID(feedback.User.Id), _itemsMap.ToInternalID(feedback.Item.Id));
				}
				((ItemRecommender)_mmlRecommenderInstance).Feedback = mmlFeedback;
			}

			Logger.Current.Trace("Training with MyMediaLite recommender...");
			PureTrainTime = (int)Wrap.MeasureTime(delegate() { _mmlRecommenderInstance.Train(); }).TotalMilliseconds;
		}

		public override void Evaluate(Split split, EvaluationContext context)
		{
			PureEvaluationTime = (int)Wrap.MeasureTime(delegate()
			{
				foreach (var feedback in split.Test)
				{
					context.PredictedScores.Add(feedback, Predict(feedback));
				}

				context.Evaluators.ForEach(e => e.Evaluate(context, this, split));
			}).TotalMilliseconds;
		}

		public override void Clear()
		{
			_usersMap = new Mapping();
			_itemsMap = new Mapping();
		}

		public float Predict(Feedback feedback)
		{
			return _mmlRecommenderInstance.Predict(_usersMap.ToInternalID(feedback.User.Id), _itemsMap.ToInternalID(feedback.Item.Id));
		}

		public override Dictionary<string, string> GetModelParameters()
		{
			string mlClass = SetupParameters["ml-class"].Split('.').Last();
			return SetupParameters.Select(kv =>
			{
				if (kv.Key == "ml-class")
					return new KeyValuePair<string, string>("ml-class", mlClass);
				return kv;
			}).ToDictionary(kv => kv.Key, kv => kv.Value);
		}
	}
}
