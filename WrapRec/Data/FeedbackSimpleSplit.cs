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
    public class FeedbackSimpleSplit : Split
    {
		public override void Setup()
		{
			// the CrossValidationSplit is already setuped by its parent split
			if (Type == SplitType.CROSSVALIDATION_SPLIT)
				return;

			// Loading DataContainer
			Readers.ForEach(r => r.LoadData(Container));

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
				int trainCount = Convert.ToInt32(Container.Feedbacks.Count * trainRatio);

				_train = feedbacks.Take(trainCount);
				_test = feedbacks.Skip(trainCount);
			}
			else if (Type == SplitType.CROSSVALIDATION)
			{
				var feedbacks = Container.Feedbacks.Shuffle();
				int foldCount = (int)((1f / numFolds) * Container.Feedbacks.Count);

				SubSplits = Enumerable.Range(0, numFolds)
					.Select(i =>
					{
						var train = feedbacks.Take((numFolds - i - 1) * foldCount)
							.Concat(feedbacks.Skip((numFolds - i) * foldCount)
							.Take(i * foldCount));
						var test = feedbacks.Skip((numFolds - i - 1) * foldCount)
							.Take(foldCount);
						return new FeedbackSimpleSplit(train, test) 
						{ 
							Id = this.Id + "-fold" + (i + 1),
							Type = SplitType.CROSSVALIDATION_SPLIT
						};
					});
			}
		}

        public FeedbackSimpleSplit() { }
	
		public FeedbackSimpleSplit(IEnumerable<Feedback> train, IEnumerable<Feedback> test)
		{
			_train = train;
			_test = test;
		}

    }
}
