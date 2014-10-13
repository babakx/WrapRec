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
        public ICollection<PositiveFeedback> PositiveFeedbacks { get; private set; }

        public DataContainer()
        {
            Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Ratings = new HashSet<ItemRating>();
            PositiveFeedbacks = new HashSet<PositiveFeedback>();
        }

        public virtual ItemRating AddRating(string userId, string itemId, float rating, bool isTest = false)
        {
            User u = AddUser(userId);
            Item i = AddItem(itemId);

            var ir = new ItemRating(u, i, rating);
            ir.IsTest = isTest;

            Ratings.Add(ir);
            u.Ratings.Add(ir);
            i.Ratings.Add(ir);

            return ir;
        }

        public virtual PositiveFeedback AddPositiveFeedback(string userId, string itemId, bool isTest = false)
        {
            User u = AddUser(userId);
            Item i = AddItem(itemId);

            var pf = new PositiveFeedback(u, i);
            pf.IsTest = false;

            PositiveFeedbacks.Add(pf);
            u.PositiveFeedbacks.Add(pf);
            i.PositiveFeedbacks.Add(pf);

            return pf;
        }

        public User AddUser(string userId)
        {
            User u;

            if (!Users.TryGetValue(userId, out u))
            {
                u = new User(userId);
                Users.Add(userId, u);
            }

            return u;
        }

        public Item AddItem(string itemId)
        {
            Item i;

            if (!Items.TryGetValue(itemId, out i))
            {
                i = new Item(itemId);
                Items.Add(itemId, i);
            }

            return i;
        }

        public override string ToString()
        {
            return string.Format("{0} Users, {1} Items, {2} Ratings", Users.Count, Items.Count, Ratings.Count);
        }

    }
}
