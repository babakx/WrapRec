using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class User : MapEnabledEntity
    {
        public ICollection<ItemRating> Ratings { get; private set; }

        public User(string id)
            : base(id)
        {
            Ratings = new HashSet<ItemRating>();
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, {1} Ratings, {2}", Id, Ratings.Count, GetRatings());
        }

        public string GetRatings()
        {
            return Ratings.GroupBy(r => r.Domain)
                .Select(d => d.Key.Id + " > " + d.Select(dr => string.Format("{0},{1}", dr.Item.Id, dr.Rating))
                    .Aggregate((cur, next) => cur + " " + next))
                .Aggregate((cur, next) => cur + " | " + next);
        }
    }
}
