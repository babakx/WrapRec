using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;

namespace RF2
{
    public class ItemRating<T> : UserItem
    {
        public T Rating { get; set; }
        public T PredictedRating { get; set; }

        public ItemRating()
        { }
        
        public ItemRating(string userId, string itemId)
            : base(userId, itemId)
        { }

        public ItemRating(string userId, string itemId, T rating)
            : this(userId, itemId)
        {
            Rating = rating; 
        }

        public ItemRating(User user, Item item, T rating)
            : base(user, item)
        {
            Rating = rating;
        }

        public virtual string ToLibFmFeatureVector(Mapping usersItemsMap)
        {
            return string.Format("{0} {1}:1 {2}:1", Rating, usersItemsMap.ToInternalID(User.Id), usersItemsMap.ToInternalID(Item.Id));
        }
    }

    public class ItemRating : ItemRating<float>
    {
        public ItemRating()
        { }
        
        public ItemRating(string userId, string itemId)
            : base(userId, itemId)
        { }

        public ItemRating(string userId, string itemId, float rating)
            : base(userId, itemId, rating)
        { }

        public ItemRating(User user, Item item, float rating)
            : base(user, item, rating)
        { }

    }
}
