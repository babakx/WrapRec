using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class DataContext
    {
        public Dictionary<string, User> Users { get; set; }
        public Dictionary<string, Item> Items { get; set; }
        public ICollection<Rating> Ratings { get; set; }

        public DataContext()
        {
            Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Ratings = new HashSet<Rating>();
        }

        public void AddRating(string userId, string itemId, float rating)
        {
            User u;

            if (!Users.TryGetValue(userId, out u))
            {
                u = new User();
            }
        }
    }
}
