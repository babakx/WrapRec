using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;
using System.IO;
using WrapRec.IO;
using WrapRec.Utils;

namespace WrapRec.Data
{
    public class DataContainer
    {
		public string Id { get; set; }
		public List<DatasetReader> DataReaders { get; private set; }
		public bool IsLoaded { get; private set; }
		public Dictionary<string, User> Users { get; private set; }
        public Dictionary<string, Item> Items { get; private set; }
        public HashSet<Feedback> Feedbacks { get; private set; }
		public MultiKeyDictionary<string, string, Feedback> FeedbacksDic { get; private set; }
		public bool AllowDuplicates { get; set; }

        protected Dictionary<string, string> _statistics;

		float _minTarget = 1, _maxTarget = 1;

        public DataContainer()
        {
			DataReaders = new List<DatasetReader>();
			Users = new Dictionary<string, User>();
            Items = new Dictionary<string, Item>();
            Feedbacks = new HashSet<Feedback>();
			FeedbacksDic = new MultiKeyDictionary<string, string, Feedback>();
        }

		public void Load()
		{
			if (!IsLoaded)
				DataReaders.ForEach(dr => 
				{ 
					dr.Setup();
					dr.LoadData(this);
				});
			IsLoaded = true;
		}

		public void SaveAsRating(string path)
		{
			var header = new string[] { "UserId,ItemId,Rating" };
			var output = Feedbacks.Select(f => string.Format("{0},{1},{2:0.00000}", f.User.Id, f.Item.Id, ((Rating)f).Value));
			File.WriteAllLines(path, header.Concat(output));
		}

        public Feedback AddFeedback(string userId, string itemId)
        {
			Feedback feedback;
			
			if (!AllowDuplicates && FeedbacksDic.ContainsKey(userId, itemId))
			{
				feedback = FeedbacksDic[userId, itemId];
				// promote a feedback to a higher level if it is already observed 
				// note that this assumes that the feedback are positive
				//feedback.Level++;
				return feedback;
			}

			User u = AddUser(userId);
            Item i = AddItem(itemId);

            feedback = new Feedback(u, i);

			Feedbacks.Add(feedback);
			u.Feedbacks.Add(feedback);
			i.Feedbacks.Add(feedback);
			
			FeedbacksDic.Add(userId, itemId, feedback);

			return feedback;
        }

		public void RemoveFeedback(Feedback f)
		{
			f.User.Feedbacks.Remove(f);
			f.Item.Feedbacks.Remove(f);

			Feedbacks.Remove(f);
			FeedbacksDic[f.User.Id].Remove(f.Item.Id);

			if (f.User.Feedbacks.Count == 0)
				Users.Remove(f.User.Id);

			if (f.Item.Feedbacks.Count == 0)
				Items.Remove(f.Item.Id);
		}

        public void RemoveItem(Item item)
        {
            foreach (Feedback f in item.Feedbacks)
            {
                Feedbacks.Remove(f);
                FeedbacksDic[f.User.Id].Remove(f.Item.Id);

                if (f.User.Feedbacks.Count == 0)
                    Users.Remove(f.User.Id);
            }

            Items.Remove(item.Id);
        }

        public Rating AddRating(string userId, string itemId, float rating)
        {
			if (rating > _maxTarget)
				_maxTarget = rating;
			
			if (rating < _minTarget)
				_minTarget = rating;
			
			User u = AddUser(userId);
            Item i = AddItem(itemId);

            var r = new Rating(u, i, rating);

            Feedbacks.Add(r);
            u.Feedbacks.Add(r);
            i.Feedbacks.Add(r);

			FeedbacksDic.Add(userId, itemId, r);

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

		public float MinTarget
		{
			get { return _minTarget; }
		}

		public float MaxTarget
		{
			get { return _maxTarget; }
		}

        public override string ToString()
        {
            return string.Format("{0} Users, {1} Items, {2} Feedbacks", Users.Count, Items.Count, Feedbacks.Count);
        }

        public Dictionary<string, string> GetStatistics()
        {
            if (_statistics != null)
                return _statistics;

            Logger.Current.Info("Calculating dataContainer '{0}' statistics...", Id);
            _statistics = new Dictionary<string, string>();

            long matrixCount = (long)Users.Count * Items.Count;
            double sparsity = (double)100L * (matrixCount - Feedbacks.Count) / matrixCount;

            var users = new Dictionary<string, int>();
            var items = new Dictionary<string, int>();
			
            foreach (Feedback f in Feedbacks)
            {
                string userId = f.User.Id;
                string itemId = f.Item.Id;

                if (!users.ContainsKey(userId))
                    users[userId] = 1;
                else
                    users[userId]++;

                if (!items.ContainsKey(itemId))
                    items[itemId] = 1;
                else
                    items[itemId]++;
            }

			var feedbackAttrs = Feedbacks.First().Attributes.Values.Select(a => a.Name).Distinct().ToList();
			var userAttrs = Feedbacks.First().User.Attributes.Values.Select(a => a.Name).Distinct().ToList();
			var itemAttrs = Feedbacks.First().Item.Attributes.Values.Select(a => a.Name).Distinct().ToList();

            _statistics.Add("containerId", Id);

            _statistics.Add("feedbacks", Feedbacks.Count.ToString());
            _statistics.Add("users", Users.Count.ToString());
            _statistics.Add("items", Items.Count.ToString());
            _statistics.Add("sparsity", string.Format("{0:0.00}", sparsity));

            _statistics.Add("usrMinFb", users.Values.Min().ToString());
            _statistics.Add("usrMaxFb", users.Values.Max().ToString());
            _statistics.Add("usrAvgFb", string.Format("{0:0.00}", users.Values.Average()));

            _statistics.Add("itmMinFb", items.Values.Min().ToString());
            _statistics.Add("itmMaxFb", items.Values.Max().ToString());
            _statistics.Add("itmAvgFb", string.Format("{0:0.00}", items.Values.Average()));

			_statistics.Add("allFeedbackAttrs", feedbackAttrs.Count > 0 ? feedbackAttrs.Aggregate((a, b) => a + "|" + b) : "NA");
			_statistics.Add("allUserAttrs", userAttrs.Count > 0 ? userAttrs.Aggregate((a, b) => a + "|" + b) : "NA");
			_statistics.Add("allItemAttrs", itemAttrs.Count > 0 ? itemAttrs.Aggregate((a, b) => a + "|" + b) : "NA");

            return _statistics;
        }
    }
}
