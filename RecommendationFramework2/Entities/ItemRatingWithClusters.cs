using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Entities
{
    public class ItemRatingWithClusters : ItemRating
    {
        public string UserCluster { get; set; }

        public string ItemCluster { get; set; }

        public string AuxUserCluster { get; set; }

        public string AuxItemCluster { get; set; }

        public ItemRatingWithClusters() 
        {
            
        }

        public override string ToLibFmFeatureVector(Mapping usersItemsMap)
        {
            string userClusterFeature = "", itemClusterFeature = "", aUserClusterFeature = "", aItemClusterFeature = "";
            
            if (!string.IsNullOrEmpty(UserCluster))
                userClusterFeature = usersItemsMap.ToInternalID(UserCluster).ToString();

            if (!string.IsNullOrEmpty(ItemCluster))
                itemClusterFeature = usersItemsMap.ToInternalID(ItemCluster).ToString();

            if (!string.IsNullOrEmpty(AuxUserCluster))
                aUserClusterFeature = usersItemsMap.ToInternalID(AuxUserCluster).ToString();

            if (!string.IsNullOrEmpty(AuxItemCluster))
                aItemClusterFeature = usersItemsMap.ToInternalID(AuxItemCluster).ToString();


            string featVector = string.Format("{0} {1}:1 {2}:1",
                Rating,
                usersItemsMap.ToInternalID(User.Id),
                usersItemsMap.ToInternalID(Item.Id));

            if (userClusterFeature != "")
                featVector += " " + userClusterFeature + ":1";

            if (itemClusterFeature != "")
                featVector += " " + itemClusterFeature + ":1";

            if (aUserClusterFeature != "")
                featVector += " " + aUserClusterFeature + ":1";

            if (aItemClusterFeature != "")
                featVector += " " + aItemClusterFeature + ":1";

            return featVector;
        }

    }
}
