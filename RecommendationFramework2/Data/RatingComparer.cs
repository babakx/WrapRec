using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class RatingComparer : IEqualityComparer<ItemRating>
    {
        public bool Equals(ItemRating x, ItemRating y)
        {
            return (x.Item.Id == y.Item.Id && x.User.Id == y.User.Id);
        }

        public int GetHashCode(ItemRating obj)
        {
            return obj.GetHashCode();
        }
    }
}
