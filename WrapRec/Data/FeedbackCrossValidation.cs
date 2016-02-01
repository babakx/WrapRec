using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class FeedbackCrossValidation : Split
    {
        // Early-load constructor: the container should be loaded in advance
		public FeedbackCrossValidation(int numFolds)
        {
			var feedbacks = Container.Feedbacks.Shuffle();
			int foldCount = (int) ((1f / numFolds) * Container.Feedbacks.Count);

			SubSplits = Enumerable.Range(0, numFolds)
				.Select(i =>
				{
					var train = feedbacks.Take((numFolds - i - 1) * foldCount)
						.Concat(feedbacks.Skip((numFolds - i) * foldCount)
						.Take(i * foldCount));
					var test = feedbacks.Skip((numFolds - i - 1) * foldCount)
						.Take(foldCount);
					return new FeedbackSimpleSplit(train, test) { Id = this.Id + "-fold" + i };
				});
		}
    }
}
