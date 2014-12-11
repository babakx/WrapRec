using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using System.IO;

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
            _itemsCluster = new Dictionary<string, int>();

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

        public void CreateDomainsBasedOnDate()
        {
            int count = Ratings.Count;
            var ratings = Ratings.OrderBy(r => long.Parse(r.GetProperty("timestamp")));
            int ratingPerDomain = count / NumDomains;

            for (int i = 0; i < NumDomains; i++)
            {
                foreach (var r in ratings.Skip(i * ratingPerDomain).Take(ratingPerDomain))
                {
                    r.Domain = Domains["ml" + i];
                }
            }

            foreach (var r in ratings.Skip(NumDomains * ratingPerDomain))
            {
                r.Domain = Domains["ml" + (NumDomains - 1)];
            }

        }

        public override ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            ActiveDomain = GetItemDomain(itemId);
            return base.AddRating(userId, itemId, rating, isTest);
        }

        public Domain GetItemDomain(string itemId)
        {
            // default cluster is 0
            int clusterId = 0;
            _itemsCluster.TryGetValue(itemId, out clusterId);

            return Domains["ml" + clusterId];
        }

        public void WriteClusters(string path)
        {
            var output = Items.Values.Select(i => 
                string.Format("{0},{1},{2}", i.Id, i.GetProperty("genres"), GetItemDomain(i.Id).Id.Substring(2)));

            File.WriteAllLines(path, output);
        }
    }
}
