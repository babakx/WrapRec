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
        public ICollection<UserItem> PositiveFeedbacks { get; private set; }

        public DataContainer()
        {
            Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Ratings = new HashSet<ItemRating>();
            PositiveFeedbacks = new HashSet<UserItem>();
        }

        public virtual ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
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

        public virtual UserItem AddPositiveFeedback(string userId, string itemId)
        {
            User u = AddUser(userId);
            Item i = AddItem(itemId);

            var ui = new UserItem(u, i);

            PositiveFeedbacks.Add(ui);

            return ui;
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
