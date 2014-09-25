using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class DataContainer
    {
        public Dictionary<string, User> Users { get; private set; }
        public Dictionary<string, Item> Items { get; private set; }
        public ICollection<ItemRating> Ratings { get; private set; }

        public DataContainer()
        {
            Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Ratings = new HashSet<ItemRating>();
        }

        public virtual ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            User u;
            Item i;

            if (!Users.TryGetValue(userId, out u))
            {
                u = new User(userId);
                Users.Add(userId, u);
            }

            if (!Items.TryGetValue(itemId, out i))
            {
                i = new Item(itemId);
                Items.Add(itemId, i);
            }

            var ir = new ItemRating(u, i, rating);
            ir.IsTest = isTest;

            Ratings.Add(ir);
            u.Ratings.Add(ir);
            i.Ratings.Add(ir);

            return ir;
        }

        public override string ToString()
        {
            return string.Format("{0} Users, {1} Items, {2} Ratings", Users.Count, Items.Count, Ratings.Count);
        }

    }
}
