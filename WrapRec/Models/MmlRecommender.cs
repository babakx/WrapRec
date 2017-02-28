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
		public Mapping UsersMap { get; protected set; }
		public Mapping ItemsMap { get; protected set; }
		public IRecommender MmlRecommenderInstance { get; protected set; }

		public MmlRecommender()
        {
            UsersMap = new Mapping();
            ItemsMap = new Mapping();
        }

		public override void Setup()
		{
			try
			{
				// build recommender object
				Type mmlRecommenderType = Helpers.ResolveType(SetupParameters["ml-class"]);
				MmlRecommenderInstance = (IRecommender)mmlRecommenderType.GetConstructor(Type.EmptyTypes).Invoke(null);

				if (typeof(IRatingPredictor).IsAssignableFrom(mmlRecommenderType))
					DataType = DataType.Ratings;
				else if (typeof(ItemRecommender).IsAssignableFrom(mmlRecommenderType))
					DataType = DataType.PosFeedback;
				else
					throw new WrapRecException(string.Format("Unknown MmlRecommender class: {0}", SetupParameters["ml-class"]));

				// Set properties
				foreach (var param in SetupParameters.Where(kv => kv.Key != "ml-class"))
				{
					PropertyInfo pi = MmlRecommenderInstance.GetType().GetProperty(param.Key);

					// in case the value of attribute is empty ignore
					// empty attributes are only used for logging purposes
					if (String.IsNullOrEmpty(param.Value))
						continue;
					
					object paramVal;
                    if (pi.PropertyType.IsEnum)
                        paramVal = Enum.Parse(pi.PropertyType, param.Value);
                    else
                        paramVal = param.Value;

                    pi.SetValue(MmlRecommenderInstance, Convert.ChangeType(paramVal, pi.PropertyType));
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
					mmlFeedback.Add(UsersMap.ToInternalID(rating.User.Id), ItemsMap.ToInternalID(rating.Item.Id), rating.Value);
				}
				((IRatingPredictor)MmlRecommenderInstance).Ratings = mmlFeedback;
			}
			else
			{
				var mmlFeedback = new PosOnlyFeedback<SparseBooleanMatrix>();
				foreach (var feedback in split.Train)
				{
					mmlFeedback.Add(UsersMap.ToInternalID(feedback.User.Id), ItemsMap.ToInternalID(feedback.Item.Id));
				}
				((ItemRecommender)MmlRecommenderInstance).Feedback = mmlFeedback;

                if (MmlRecommenderInstance is IModelAwareRecommender)
                    ((IModelAwareRecommender)MmlRecommenderInstance).Model = this;
			}

            Logger.Current.Trace("Training with MyMediaLite recommender...");
			PureTrainTime = (int)Wrap.MeasureTime(delegate() { MmlRecommenderInstance.Train(); }).TotalMilliseconds;
		}

		public override void Evaluate(Split split, EvaluationContext context)
		{
			ExhaustInternalIds(split);
			
			PureEvaluationTime = (int)Wrap.MeasureTime(delegate()
			{
				if (DataType == DataType.Ratings)
					foreach (var feedback in split.Test)
						context.PredictedScores.Add(feedback, Predict(feedback));
	
				context.Evaluators.ForEach(e => e.Evaluate(context, this, split));
			}).TotalMilliseconds;
		}

		// This method makes sure that all itemIds are already have an internalId when they want to be used in evaluation
		// this prevent cross-thread access to ItemMap (already existing key in dictionary error)
		// when evaluation is peformed in parallel for each user
		private void ExhaustInternalIds(Split split)
		{
			foreach (var item in split.Container.Items.Values)
				ItemsMap.ToInternalID(item.Id);

			foreach (var user in split.Container.Items.Values)
				UsersMap.ToInternalID(user.Id);
		}

		public override void Clear()
		{
			UsersMap = new Mapping();
			ItemsMap = new Mapping();
		}

		public override float Predict(string userId, string itemId)
		{
			return MmlRecommenderInstance.Predict(UsersMap.ToInternalID(userId), ItemsMap.ToInternalID(itemId));
		}

		public override float Predict(Feedback feedback)
		{
			return Predict(feedback.User.Id, feedback.Item.Id);
		}

		public override Dictionary<string, string> GetModelParameters()
		{
			string mlClass = SetupParameters["ml-class"].Split('.').Last();
			return SetupParameters.Select(kv =>
			{
				if (kv.Key == "ml-class")
					return new KeyValuePair<string, string>("ml-class", mlClass);

				if (string.IsNullOrEmpty(kv.Value))
					return new KeyValuePair<string, string>(kv.Key,
						MmlRecommenderInstance.GetType().GetProperty(kv.Key).GetValue(MmlRecommenderInstance).ToString());

				return kv;
			}).ToDictionary(kv => kv.Key, kv => kv.Value);
		}

	}
}
