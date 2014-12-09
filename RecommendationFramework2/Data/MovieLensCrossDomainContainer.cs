using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;

namespace WrapRec.Data
{
    public class MovieLensCrossDomainContainer : CrossDomainDataContainer
    {
        Mapping _mapper;
        Dictionary<string, int> _itemsCluster;

        public int NumDomains { get; set; }
        
        public MovieLensCrossDomainContainer(int numDomains)
            : base()
        {
            _mapper = new Mapping();
            NumDomains = numDomains;

            for (int i = 0; i < numDomains; i++)
            { 
                string dId = "ml" + i;
                Domains.Add(dId, new Domain(dId));
            }
        }

        public void CreateItemClusters()
        { 
            // this is equal to the number of possible genres (in the MovieLens 1M dataset that is 18)
            int numFeatures = 18;
            double[,] genreFeatures = new double[Items.Count, numFeatures];
            var itemIds = new List<string>();

            int i = 0;
            foreach (var item in Items.Values)
            {
                itemIds.Add(item.Id);

                string genresStr = item.Properties["genres"];
                var genres = genresStr.Split('|');
                
                foreach (string g in genres)
	            {
                    genreFeatures[i, _mapper.ToInternalID(g)] = 1;
	            }

                i++;
            }

            _itemsCluster = Clusterer.Cluster(itemIds, genreFeatures, NumDomains);
        }

        public override ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            ActiveDomain = GetItemDomain(itemId);
            return base.AddRating(userId, itemId, rating, isTest);
        }

        public Domain GetItemDomain(string itemId)
        {
            return Domains["ml" + _itemsCluster[itemId]];
        }
    }
}
