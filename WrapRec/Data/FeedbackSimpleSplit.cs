using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;
using System.IO;
using WrapRec.Utils;

namespace WrapRec.Data
{
    public class FeedbackSimpleSplit : Split
    {
		public override void Setup()
		{
			// the CrossValidationSplit is already setuped by its parent split
			if (IsSetup || Type == SplitType.CROSSVALIDATION_SPLIT)
				return;

			// Setuping splits
			float trainRatio = 1f;
			int numFolds = 5;

			if (SetupParameters.ContainsKey("trainRatio"))
				trainRatio = float.Parse(SetupParameters["trainRatio"]);

			if (SetupParameters.ContainsKey("numFolds"))
				numFolds = int.Parse(SetupParameters["numFolds"]);

			if (Type == SplitType.STATIC)
			{
				_train = Container.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TRAIN);
				_test = Container.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TEST || f.SliceType == FeedbackSlice.TEST_CANDIDATE);
			}
			else if (Type == SplitType.DYNAMIC)
			{
				var feedbacks = Container.Feedbacks.Shuffle();
				// the trainCount wont be calculated until the enumerator is being used
				// So container is not required to be loaded in advanced
				var trainCount = new Lazy<int>(() => Convert.ToInt32(Container.Feedbacks.Count * trainRatio));

				_train = feedbacks.Take(trainCount);
				_test = feedbacks.Skip(trainCount);
			}
			else if (Type == SplitType.CROSSVALIDATION)
			{
				var feedbacks = Container.Feedbacks.Shuffle();
				// here all parameters of Take and Skip functions are calculated with lazyLoading
				// The SubSplits are formed when the enumeration is being started
				var foldCount = new Lazy<int>(() => (int)((1f / numFolds) * Container.Feedbacks.Count));

				SubSplits = Enumerable.Range(0, numFolds)
					.Select(i =>
					{
						var train = feedbacks.Take(() => (numFolds - i - 1) * foldCount.Value)
							.Concat(feedbacks.Skip(() => (numFolds - i) * foldCount.Value)
							.Take(() => i * foldCount.Value));
						var test = feedbacks.Skip(() => (numFolds - i - 1) * foldCount.Value)
							.Take(foldCount);
						return new FeedbackSimpleSplit(train, test) 
						{ 
							Id = this.Id + "-fold" + (i + 1),
							Type = SplitType.CROSSVALIDATION_SPLIT,
							Container = this.Container
						};
					});
			}
			IsSetup = true;
		}

        public FeedbackSimpleSplit() { }
	
		public FeedbackSimpleSplit(IEnumerable<Feedback> train, IEnumerable<Feedback> test)
		{
			_train = train;
			_test = test;
		}

    }
}
