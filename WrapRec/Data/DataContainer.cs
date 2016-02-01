using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;
using System.IO;

namespace WrapRec.Data
{
    public class DataContainer
    {
        public Dictionary<string, User> Users { get; private set; }
        public Dictionary<string, Item> Items { get; private set; }
        public HashSet<Feedback> Feedbacks { get; private set; }

        static DataContainer _instance;

        private DataContainer()
        {
            Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Feedbacks = new HashSet<Feedback>();
        }

        public static DataContainer GetInstance()
        {
            if (_instance == null)
                _instance = new DataContainer();

            return _instance;
        }

		public void SaveAsRating(string path)
		{
			var header = new string[] { "UserId,ItemId,Rating" };
			var output = Feedbacks.Select(f => string.Format("{0},{1},{2:0.00000}", f.User.Id, f.Item.Id, ((Rating)f).Value));
			File.WriteAllLines(path, header.Concat(output));
		}

        public Feedback AddFeedback(string userId, string itemId)
        {
            User u = AddUser(userId);
            Item i = AddItem(itemId);

            var f = new Feedback(u, i);

            Feedbacks.Add(f);
            u.Feedbacks.Add(f);
            i.Feedbacks.Add(f);

            return f;
        }

		public void RemoveFeedback(Feedback f)
		{
			f.User.Feedbacks.Remove(f);
			f.Item.Feedbacks.Remove(f);

			Feedbacks.Remove(f);

			if (f.User.Feedbacks.Count == 0)
				Users.Remove(f.User.Id);

			if (f.Item.Feedbacks.Count == 0)
				Items.Remove(f.Item.Id);
		}

        public Rating AddRating(string userId, string itemId, float rating)
        {
            User u = AddUser(userId);
            Item i = AddItem(itemId);

            var r = new Rating(u, i, rating);

            Feedbacks.Add(r);
            u.Feedbacks.Add(r);
            i.Feedbacks.Add(r);

            return r;
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

        /// <summary>
        /// Desify DataContainer by making sure that each user has at least k items and each item has at least k users
        /// </summary>
        /// <param name="k"></param>
        public void Densify(int k)
        {
            int removedBasedOnUser, removedBasedOnItem;
            int i = 1;

            Logger.Current.Info("Densify with min feedback {0}...", k);
            Logger.Current.Trace(ToString());

            do
            {
                Logger.Current.Trace("Iteration {0}: ", i);

                var toRemove = Feedbacks.GroupBy(f => f.User).Where(g => g.Count() < k)
                    .SelectMany(g => g);

                removedBasedOnUser = 0;
                foreach (Feedback f in toRemove)
                {
                    RemoveFeedback(f);
                    removedBasedOnUser++;
                }

                Logger.Current.Trace("Removed {0} (user-based)", removedBasedOnUser);
                Logger.Current.Trace(ToString());

                toRemove = Feedbacks.GroupBy(f => f.Item).Where(g => g.Count() < k)
                    .SelectMany(g => g);

                removedBasedOnItem = 0;
                foreach (Feedback f in toRemove)
                {
                    RemoveFeedback(f);
                    removedBasedOnItem++;
                }

                Logger.Current.Trace("Removed {0} (item-based)", removedBasedOnItem);
                Logger.Current.Trace(ToString() + "\n");
                i++;
            }
            while (removedBasedOnUser > 0 || removedBasedOnItem > 0);

            Logger.Current.Info("Densify complete with min feedback {0}", k);
        }

        public override string ToString()
        {
            return string.Format("{0} Users, {1} Items, {2} Feedbacks", Users.Count, Items.Count, Feedbacks.Count);
        }
    }
}
