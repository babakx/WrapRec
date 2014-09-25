using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using WrapRec.Data;
using MyMediaLite.DataType;

namespace WrapRec.Recommenders
{
    public class MediaLiteRatingPredictor : IPredictor<ItemRating>
    {
        IRatingPredictor _ratingPredictor;
        Mapping _usersMap;
        Mapping _itemsMap;
        bool _isTrained;
        SparseBooleanMatrix _relations;

        public MediaLiteRatingPredictor(MyMediaLite.IRecommender recommender)
        {
            _ratingPredictor = recommender as MyMediaLite.RatingPrediction.IRatingPredictor;
            _usersMap = new Mapping();
            _itemsMap = new Mapping();
        }

        public MediaLiteRatingPredictor(MyMediaLite.IRecommender recommender, IEnumerable<Relation> relations)
            : this(recommender) 
        {
            if (recommender is SocialMF)
            {
                _relations = new SparseBooleanMatrix();

                foreach (var con in relations)
                {
                    _relations[_usersMap.ToInternalID(con.UserId), _usersMap.ToInternalID(con.ConnectedId)] = true;
                }

                ((SocialMF)recommender).UserRelation = _relations;
            }
        }

        public void Train(IEnumerable<ItemRating> trainSet)
        {
            Console.WriteLine("Training...");

            var ratings = new MyMediaLite.Data.Ratings();

            // Convert trainset to MyMediaLite trianset format
            foreach (var itemRating in trainSet)
            {
                ratings.Add(_usersMap.ToInternalID(itemRating.User.Id), _itemsMap.ToInternalID(itemRating.Item.Id), itemRating.Rating);
            }

            _ratingPredictor.Ratings = ratings;
            _ratingPredictor.Train();

            _isTrained = true;
        }

        public void Predict(ItemRating sample)
        {
            sample.PredictedRating = _ratingPredictor.Predict(_usersMap.ToInternalID(sample.User.Id), _itemsMap.ToInternalID(sample.Item.Id));
        }
        
        public bool IsTrained
        {
            get
            {
                return _isTrained;
            }
        }
    }
}
