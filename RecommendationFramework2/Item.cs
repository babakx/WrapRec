using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class Item : MapEnabledEntity
    {
        public ICollection<User> RatingUsers { get; set; }
        public ICollection<Item> RankingUsers { get; set; }

        public Item(string id)
            : base(id)
        { }
    }
}
