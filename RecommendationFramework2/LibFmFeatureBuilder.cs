using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class LibFmFeatureBuilder
    {
        // users and items should be in the same map because libFm views them in the same way 
        // i.e. it can not distinguesh which feature is user and which feature is item
        // therefore there shouldn't be duplicate mapped id's between users and items
        public Mapping Mapper { get; set; }

        public LibFmFeatureBuilder()
        {
            Mapper = new Mapping();        
        }

        public virtual string GetLibFmFeatureVector(ItemRating rating)
        {
            return string.Format("{0} {1}:1 {2}:1", rating.Rating, Mapper.ToInternalID(rating.User.Id), Mapper.ToInternalID(rating.Item.Id));
        }

        public virtual string GetLibFmFeatureVector(PositiveFeedback rating)
        {
            return string.Format("{0} {1}:1 {2}:1", 1, Mapper.ToInternalID(rating.User.Id), Mapper.ToInternalID(rating.Item.Id));
        }
    }
}
