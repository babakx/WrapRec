using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.IO;
using LinqLib.Sequence;

namespace WrapRec.Data
{
	public enum SplitType
	{ 
		STATIC,
		DYNAMIC,
		DYNAMIC_SUBSPLIT,
		CROSSVALIDATION,
		CROSSVALIDATION_SUBSPLIT,
		CUSTOM
	}

	public abstract class Split
	{
		public string Id { get; set; }
		public DataContainer Container { get; set; }
		public Dictionary<string, string> SetupParameters { get; set; }
		public SplitType Type { get; set; }
		public bool IsSetup { get; protected set; }

		protected IEnumerable<Feedback> _train;
        protected IEnumerable<Feedback> _test;
        protected Dictionary<string, string> _statistics;

        public IEnumerable<Split> SubSplits { get; set; }

        public IEnumerable<Feedback> Train 
		{ 
			get { return _train; } 
		}

		public IEnumerable<Feedback> Test
		{
			get { return _test; }
		}

		public virtual void Setup()
		{ 
			// empty body
		}

        public void UpdateFeedbackSlices()
        {
            foreach (Feedback f in Train)
                f.SliceType = FeedbackSlice.TRAIN;

            foreach (Feedback f in Test)
                f.SliceType = FeedbackSlice.TEST;
        }

		public IEnumerable<Feedback> SampleNegativeFeedback(int count)
		{
			UpdateFeedbackSlices();

			var users = Container.Users.Keys.ToList();
			var items = Container.Items.Keys.ToList();

			// real-valued attributes can be added to negative feedbacks with value of 0
			// here we assume that first feedback has all attributes
			var rAttrs = Container.Feedbacks.First().Attributes.Values
				.Where(a => a.Type == AttributeType.RealValued)
				.Select(a => new Core.Attribute() { Name = a.Name, Type = AttributeType.RealValued, Value = "0" });

			int i = 0;
			while (i < count)
			{
				var user = SampleUser(users);
				var item = SampleItem(items);

				var userTrainItemIds = user.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TRAIN).Select(f => f.Item.Id);

				if (!userTrainItemIds.Contains(item.Id))
				{
					i++;
					var f = new Feedback(user, item);
					f.FeedbackType = FeedbackType.Negative;

					foreach (var attr in rAttrs)
						f.Attributes.Add(attr.Name, attr);

					yield return f;
				}
			}
		}

	    private User SampleUser(List<string> allUserIds)
		{
			int rndIndex = (new Random()).Next(allUserIds.Count);
			return Container.Users[allUserIds[rndIndex]];
		}

		private Item SampleItem(List<string> allItemIds)
		{
			int rndIndex = (new Random()).Next(allItemIds.Count);
			return Container.Items[allItemIds[rndIndex]];
		}

        public Dictionary<string, string> GetStatistics()
        {
            // TODO: when test is empty ther is error
            if (_statistics != null)
                return _statistics;

            Logger.Current.Info("Calculating split '{0}' statistics...", Id);

            var trainUsers = new Dictionary<string, int>();
            var trainItems = new Dictionary<string, int>();

            var testUsers = new Dictionary<string, int>();
            var testItems = new Dictionary<string, int>();

            int trainCount = 0, testCount = 0;

            foreach (Feedback f in Train)
            {
                trainCount++;
                string userId = f.User.Id;
                string itemId = f.Item.Id;

                if (!trainUsers.ContainsKey(userId))
                    trainUsers[userId] = 1;
                else
                    trainUsers[userId]++;

                if (!trainItems.ContainsKey(itemId))
                    trainItems[itemId] = 1;
                else
                    trainItems[itemId]++;
            }

            foreach (Feedback f in Test)
            {
                testCount++;
                string userId = f.User.Id;
                string itemId = f.Item.Id;

                if (!testUsers.ContainsKey(userId))
                    testUsers[userId] = 1;
                else
                    testUsers[userId]++;

                if (!testItems.ContainsKey(itemId))
                    testItems[itemId] = 1;
                else
                    testItems[itemId]++;
            }

            _statistics = new Dictionary<string, string>();

            int totalCount = trainCount + testCount;
            float percTrain = 100f * ((float)trainCount / totalCount);
            float percTest = 100f * ((float)testCount / totalCount);

            long trainMatrixCount = (long)trainUsers.Count * trainItems.Count;
            double sparsityTrain = (double)100L * (trainMatrixCount - trainCount) / trainMatrixCount;

            long testMatrixCount = (long)testUsers.Count * testItems.Count;
            double sparsityTest = (double)100L * (testMatrixCount - testCount) / testMatrixCount;

            _statistics.Add("splitId", Id);

            _statistics.Add("train", trainCount.ToString());
            _statistics.Add("test", testCount.ToString());
            _statistics.Add("%train", string.Format("{0:0.00}", percTrain));
            _statistics.Add("%test", string.Format("{0:0.00}", percTest));

            _statistics.Add("trainUsers", trainUsers.Count.ToString());
            _statistics.Add("trainItems", trainItems.Count.ToString());
            _statistics.Add("sparsityTrain", string.Format("{0:0.00}", sparsityTrain));

            _statistics.Add("testUsers", testUsers.Count.ToString());
            _statistics.Add("testItems", testItems.Count.ToString());
            _statistics.Add("sparsityTest", string.Format("{0:0.00}", sparsityTest));

            _statistics.Add("newTestUsers", testUsers.Keys.Except(trainUsers.Keys).Count().ToString());
            _statistics.Add("newTestItems", testItems.Keys.Except(trainItems.Keys).Count().ToString());

            _statistics.Add("trUsrMinFb", trainUsers.Values.Min().ToString());
            _statistics.Add("trUsrMaxFb", trainUsers.Values.Max().ToString());
            _statistics.Add("trUsrAvgFb", string.Format("{0:0.00}", trainUsers.Values.Average()));

            _statistics.Add("teUsrMinFb", testUsers.Values.Min().ToString());
            _statistics.Add("teUsrMaxFb", testUsers.Values.Max().ToString());
            _statistics.Add("teUsrAvgFb", string.Format("{0:0.00}", testUsers.Values.Average()));

            _statistics.Add("trItmMinFb", trainItems.Values.Min().ToString());
            _statistics.Add("trItmMaxFb", trainItems.Values.Max().ToString());
            _statistics.Add("trItmAvgFb", string.Format("{0:0.00}", trainItems.Values.Average()));

            _statistics.Add("teItmMinFb", testItems.Values.Min().ToString());
            _statistics.Add("teItmMaxFb", testItems.Values.Max().ToString());
            _statistics.Add("teItmAvgFb", string.Format("{0:0.00}", testItems.Values.Average()));

			_statistics.Add("feedbackAttrs", SetupParameters.ContainsKey("feedbackAttributes") ? SetupParameters["feedbackAttributes"] : "NA");
			_statistics.Add("userAttrs", SetupParameters.ContainsKey("userAttributes") ? SetupParameters["userAttributes"] : "NA");
			_statistics.Add("itemAttrs", SetupParameters.ContainsKey("itemAttributes") ? SetupParameters["itemAttributes"] : "NA");

            return _statistics;
        }
    }
}
