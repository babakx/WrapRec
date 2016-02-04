using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
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

namespace WrapRec.Models
{
    public class MmlRatingPredictor : Model
    {
        Mapping _usersMap;
        Mapping _itemsMap;
        Type _mmlRpType;
        IRatingPredictor _mmlRpInstance;

        public MmlRatingPredictor()
        {
            _usersMap = new Mapping();
            _itemsMap = new Mapping();
        }

        public override void Setup()
        {
            try
            {
                // build MmlRatingPredictor
				_mmlRpType = Helpers.ResolveType(SetupParameters["ml-class"]);
                _mmlRpInstance = (IRatingPredictor) _mmlRpType.GetConstructor(Type.EmptyTypes).Invoke(null);
                
                // Set properties
				foreach (var param in SetupParameters.Where(kv => kv.Key != "ml-class"))
                {
					PropertyInfo pi = _mmlRpInstance.GetType().GetProperty(param.Key);
                    pi.SetValue(_mmlRpInstance, Convert.ChangeType(param.Value, pi.PropertyType));
                }
            }
            catch (Exception ex)
            {
                throw new WrapRecException(string.Format("Cannot resolve MmlRatingPrediction: {0}\n{1}", ex.Message, ex.StackTrace));
            }
        }

        public override void Train(Split split)
        {
            var mmlRatings = new Ratings();

            // Convert trainset to MyMediaLite trianset format
            foreach (var feedback in split.Train)
            {
                var rating = (Rating)feedback;
                mmlRatings.Add(_usersMap.ToInternalID(rating.User.Id), _itemsMap.ToInternalID(rating.Item.Id), rating.Value);
            }

            _mmlRpInstance.Ratings = mmlRatings;
            
            Logger.Current.Trace("Training with MyMediaLite RatingPredictor...");
            PureTrainTime = (int)Wrap.MeasureTime(delegate () { _mmlRpInstance.Train(); }).TotalMilliseconds;
        }

        public override void Evaluate(Split split, EvaluationContext context)
        {
            PureEvaluationTime = (int)Wrap.MeasureTime(delegate () 
            {
                foreach (var feedback in split.Test)
                {
                    var rating = (Rating)feedback;
                    context.PredictedScores.Add(rating, Predict(rating));
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
            return _mmlRpInstance.Predict(_usersMap.ToInternalID(feedback.User.Id), _itemsMap.ToInternalID(feedback.Item.Id));
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
