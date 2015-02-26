using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using System.IO;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class MovieLensCrossDomainContainer : CrossDomainDataContainer
    {
        Mapping _mapper;
        Dictionary<string, int> _itemsCluster;

        public int NumDomains { get; set; }

        public bool RandomClusters { get; set; }

        public MovieLensCrossDomainContainer(int numDomains, bool randomClusters = false)
            : base()
        {
            _mapper = new Mapping();
            NumDomains = numDomains;
            RandomClusters = randomClusters;
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

        public void CreateItemClusters(string clusterPath)
        {
            if (NumDomains <= 1)
                return;
            
            int[] numClusters = new int[] { 2, 3, 4, 5, 6, 8, 10};
            int colNo = numClusters.IndexOf(NumDomains) + 1;

            var genreSeqClusters = File.ReadAllLines(clusterPath).Skip(1).Select(l =>
            {
                var parts = l.Split('\t');
                return new { Seq = parts[0], ClusterNo = int.Parse(parts[colNo]) - 1 };
            }).ToDictionary(s => s.Seq, s => s.ClusterNo);

            _itemsCluster = Items.Values.ToDictionary(i => i.Id, i => genreSeqClusters[i.GetProperty("genres")]);
        }

        public void CreateClusterFiles(string rawVectorsFile, string matlabInputFile)
        {
            int numFeatures = 18;
            var genreSeqs = Items.Values.Select(i => i.GetProperty("genres")).Where(g => !string.IsNullOrEmpty(g)).Distinct().ToList();
            int[,] genreFeatures = new int[genreSeqs.Count, numFeatures];

            _mapper.ToInternalID("kill index 0");

            var featureVectors = genreSeqs.Select(s => 
            {
                var vector = Enumerable.Range(0, numFeatures).Select(i => "0").ToList();

                var genres = s.Split('|');

                foreach (string g in genres)
                {
                    vector[_mapper.ToInternalID(g) - 1] = "1";
                }

                return vector.Aggregate((a, b) => a + " " + b);
            });

            File.WriteAllLines(rawVectorsFile, genreSeqs);
            File.WriteAllLines(matlabInputFile, featureVectors);
        }

        public void CreateDominantGenre(string outputFile)
        {
            var genreSeqs = Items.Values.ToDictionary(i => i.Id, i => i.GetProperty("genres"));
            var genreFreq = new Dictionary<string, int>();
            var itemMostFreqGenre = new Dictionary<string, string>();

            foreach (var sq in genreSeqs.Values)
            {
                var parts = sq.Split('|');
                parts.ToList().ForEach(g => 
                {
                    if (!genreFreq.ContainsKey(g))
                    {
                        genreFreq.Add(g, 0);
                    }
                    genreFreq[g] += 1;
                });
            }

            Console.WriteLine("Genre Frequencies");
            genreFreq.Select(kv => string.Format("{0}\t{1}", kv.Key, kv.Value)).ToList().ForEach(Console.WriteLine);


            foreach (var kv in genreSeqs)
            {
                var parts = kv.Value.Split('|');
                string genre = "";
                int max = 0;

                parts.ToList().ForEach(g =>
                {
                    if (genreFreq[g] > max)
                    {
                        max = genreFreq[g];
                        genre = g;
                    }
                });

                itemMostFreqGenre.Add(kv.Key, genre);
            }

            File.WriteAllLines(outputFile, itemMostFreqGenre.Select(kv => string.Format("{0},{1}", kv.Key, kv.Value)));
            Console.WriteLine("Dominant Genres are created!");

            _itemsCluster = itemMostFreqGenre.ToDictionary(kv => kv.Key, kv => _mapper.ToInternalID(kv.Value));
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

        public void CreateDomainsWithEvenlyDistributedUsers()
        {
            int count = Ratings.Count;
            int numDomain1 = 1;

            foreach (var g in Ratings.GroupBy(r => r.User.Id).Shuffle())
            {
                int ratingPerDomain = g.Count() / NumDomains;

                //foreach (var r in g.Take(numDomain1))
                //{
                //    r.Domain = Domains["mlt"];
                //}

                for (int i = 0; i < NumDomains; i++)
                {
                    foreach (var r in g.Skip(i * ratingPerDomain + numDomain1).Take(ratingPerDomain))
                    {
                        r.Domain = Domains["ml" + i];
                    }
                }

                foreach (var r in g.Skip(NumDomains * ratingPerDomain))
                {
                    r.Domain = Domains["ml" + (NumDomains - 1)];
                }
            }
        }

        public override ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            CurrentDomain = GetItemDomain(itemId);
            return base.AddRating(userId, itemId, rating, isTest);
        }

        public Domain GetItemDomain(string itemId)
        {
            // default cluster is 0
            int clusterId = 0;

            if (RandomClusters)
            {
                var r = new Random();
                clusterId = r.Next(NumDomains);
            }
            else if (NumDomains > 1)
            {
                _itemsCluster.TryGetValue(itemId, out clusterId);
            }

            if (clusterId >= 15)
                clusterId = 14;

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
