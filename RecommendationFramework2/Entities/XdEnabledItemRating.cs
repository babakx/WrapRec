using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;

namespace WrapRec.Entities
{
    public class XDEnabledItemRating : ItemRating
    {
        public Domain TargetDomain { get; private set; }

        public IList<XDEnabledItemRating> AuxDomainsItemRatings { get; private set; }

        public XDEnabledItemRating(ItemRating itemRating, Domain targetDomain, IList<XDEnabledItemRating> auxDomainItemRatings)
            :base(itemRating.User, itemRating.Item, itemRating.Rating)
        {
            TargetDomain = targetDomain;
            AuxDomainsItemRatings = auxDomainItemRatings;
        }

        public XDEnabledItemRating(ItemRating itemRating, Domain targetDomain)
            : this(itemRating, targetDomain, new List<XDEnabledItemRating>())
        {  }


        public void AddAuxItemRating(XDEnabledItemRating itemRating)
        {
            AuxDomainsItemRatings.Add(itemRating);
        }

        public new string ToLibFmFeatureVector(Mapping usersItemsMap)
        {
            return string.Format("{0} {1}:1 {2}:1", Rating, usersItemsMap.ToInternalID(User.Id), usersItemsMap.ToInternalID(Item.Id));
        }

    }
}
