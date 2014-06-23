using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;

namespace RF2.Entities
{
    public class XDEnabledItemRating : ItemRating
    {
        public Domain Domain { get; set; }

        public XDEnabledItemRating(ItemRating itemRating, Domain domain)
            :base(itemRating.User, itemRating.Item, itemRating.Rating)
        {
            Domain = domain;
        }
        
        public new string ToLibFmFeatureVector(Mapping usersItemsMap)
        {
            return string.Format("{0} {1}:1 {2}:1", Rating, usersItemsMap.ToInternalID(User.Id), usersItemsMap.ToInternalID(Item.Id));
        }
    }
}
