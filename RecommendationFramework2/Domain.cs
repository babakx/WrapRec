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

        public bool IsTarget { get; set; }

        public Domain(string id, bool isTarget = false, float weight = 1)
        {
            Id = id;
            Weight = weight;
            IsTarget = isTarget;
            Ratings = new HashSet<ItemRating>();
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
