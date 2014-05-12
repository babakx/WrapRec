using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.DataType;

namespace RF2.Recommenders
{
    public class MediaLiteItemRecommender : IPredictor<ItemRanking>, IItemRecommender, IUserItemMapper
    {
        ItemRecommender _itemRecommender;
        Mapping _usersMap;
        Mapping _itemsMap;
        bool _isTrained;

        public int NumRecommendations { get; set; }

        public MediaLiteItemRecommender(ItemRecommender itemRecommender)
            : this(itemRecommender, -1)
        { }

        public MediaLiteItemRecommender(ItemRecommender itemRecommender, int numRecommendations)
        {
            _itemRecommender = itemRecommender;
            _usersMap = new Mapping();
            _itemsMap = new Mapping();
            NumRecommendations = numRecommendations;
        }
        
        public void Train(IEnumerable<ItemRanking> trainSet)
        {
            Console.WriteLine("Training...");

            _itemRecommender.Feedback = trainSet.ToPosOnlyFeedback(_usersMap, _itemsMap);
            _itemRecommender.Train();

            _isTrained = true;
        }

        public void Predict(ItemRanking sample)
        {
            sample.PredictedRank = _itemRecommender.Predict(_usersMap.ToInternalID(sample.User.Id), _itemsMap.ToInternalID(sample.Item.Id));
        }

        public bool IsTrained
        {
            get { return _isTrained; }
        }

        public Mapping ItemsMap 
        {
            get { return _itemsMap; } 
        }

        public Mapping UsersMap
        {
            get { return _usersMap; }
        }

        public UserRankedList Recommend(User u)
        {
            var rankedList = new UserRankedList();
            rankedList.User = u;

            var recItems = _itemRecommender.Recommend(_usersMap.ToInternalID(u.Id), NumRecommendations);

            foreach (var item in recItems)
            {
                rankedList.Items.Add(new Item(_itemsMap.ToOriginalID(item.Item1)), item.Item2);
            }

            return rankedList;
        }
    }
}
