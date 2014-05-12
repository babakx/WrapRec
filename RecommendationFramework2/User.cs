using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class User : MapEnabledEntity
    {
        public ICollection<ItemRating> ItemRatings { get; set; }
        public ICollection<ItemRanking> ItemRankings { get; set; }

        public User(string id)
            : base(id)
        { }

    }
}
