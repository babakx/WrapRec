using WrapRec.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WrapRec
{
    public class Domain
    {
        public string Id { get; set; }
        public float Weight { get; set; }
        public ICollection<ItemRating> Ratings { get; private set; }
        public string CachedDataPath { get; set; }

        public bool IsTarget { get; set; }

        public Domain(string id, bool isTarget = false, float weight = 1, string cachedDataPath = "")
        {
            Id = id;
            Weight = weight;
            IsTarget = isTarget;
            CachedDataPath = cachedDataPath;
            Ratings = new HashSet<ItemRating>();
        }

        public void CacheUserData()
        {
            var output = Ratings.GroupBy(r => r.User.Id).Select(g => new
            {
                UserId = g.Key,
                RatingCount = g.Count(),
                Ratings = g.Select(ur => string.Format("{0}:{1}", ur.Item.Id, ur.Rating))
                    .Aggregate((cur, next) => cur + " " + next)
            }).Select(u => string.Format("{0} {1} {2}", u.UserId, u.RatingCount, u.Ratings));

            File.WriteAllLines(CachedDataPath, output);
        }

        public void ActivateData(float portion)
        {
            int count = (int)Math.Round(portion * Ratings.Count);

            foreach (var r in Ratings.Where(r => r.IsActive == false).Take(count))
            {
                r.IsActive = true;
            }

            Console.WriteLine("\nNo. of activated ratings: {0}\n", Ratings.Where(r => r.IsActive == true).Count());
        }

        public override string ToString()
        {
            int numTestSamples = Ratings.Where(r => r.IsTest == true).Count();
            int numUsers = Ratings.Select(r => r.User.Id).Distinct().Count();
            int numItems = Ratings.Select(r => r.Item.Id).Distinct().Count();
            return string.Format("{0} ({1}), {2} Users, {3} Items, {4} Ratings, {5} Test samples", 
                Id, IsTarget ? "target" : "aux", numUsers, numItems, Ratings.Count, numTestSamples);
        }

        public void WriteHistogram(string path)
        {
            var lines = Ratings.GroupBy(r => r.User.Id)
                .Select(g => new { UserId = g.Key, RatingCount = g.Count() })
                .OrderByDescending(r => r.RatingCount)
                .Select(r => string.Format("{0},{1}", r.UserId, r.RatingCount));

            File.WriteAllLines(path, lines);            
        }
    }
}
