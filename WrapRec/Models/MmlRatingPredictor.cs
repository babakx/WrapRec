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

        public override void Setup(Dictionary<string, string> modelParams)
        {
            try
            {
                // build MmlRatingPredictor
                _mmlRpType = Type.GetType(modelParams["ml-class"]);
                _mmlRpInstance = (IRatingPredictor) _mmlRpType.GetConstructor(Type.EmptyTypes).Invoke(null);
                
                // Set properties
                foreach (var param in modelParams.Where(kv => kv.Key != "ml-class"))
                {
                    PropertyInfo pi = _mmlRpType.GetType().GetProperty(param.Key);
                    pi.SetValue(_mmlRpInstance, Convert.ChangeType(param.Value, pi.PropertyType));
                }
            }
            catch (Exception ex)
            {
                throw new WrapRecException(string.Format("Cannot resolve MmlRatingPrediction parameters: {0}", ex.Message));
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

    }
}
