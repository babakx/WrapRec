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
			base.Setup();

			// the CrossValidationSplit is already setuped by its parent split
			if (IsSetup || Type == SplitType.CROSSVALIDATION_SUBSPLIT || Type == SplitType.DYNAMIC_SUBSPLIT)
			{
				IsSetup = true;
				return;
			}

			// Setuping splits
			float[] trainRatios = { 1f };
			int numFolds = 5;

			if (SetupParameters.ContainsKey("trainRatios"))
				trainRatios = SetupParameters["trainRatios"].Split(',').Select(tr => float.Parse(tr)).ToArray();

			if (SetupParameters.ContainsKey("numFolds"))
				numFolds = int.Parse(SetupParameters["numFolds"]);

			if (Type == SplitType.STATIC)
			{
				_train = Container.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TRAIN);
				_test = Container.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TEST);
			}
			else if (Type == SplitType.DYNAMIC)
			{
				var feedbacks = Container.Feedbacks.Shuffle();

				SubSplits = trainRatios.Select(tr => 
				{
					// the trainCount wont be calculated until the enumerator is being used
					// So container is not required to be loaded in advanced
					var trainCount = new Lazy<int>(() => Convert.ToInt32(Container.Feedbacks.Count * tr));

					var train = feedbacks.Take(trainCount);
					var test = feedbacks.Skip(trainCount);

					var ss = new FeedbackSimpleSplit(train, test) 
					{ 
						Id = Id + "-" + tr.ToString(),
						Type = SplitType.DYNAMIC_SUBSPLIT,
						Container = this.Container,
						SetupParameters = this.SetupParameters
					};
					ss.Setup();
					return ss;
				});
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
						var ss = new FeedbackSimpleSplit(train, test) 
						{ 
							Id = this.Id + "-fold" + (i + 1),
							Type = SplitType.CROSSVALIDATION_SUBSPLIT,
							Container = this.Container,
							SetupParameters = this.SetupParameters
						};
						ss.Setup();
						return ss;
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
