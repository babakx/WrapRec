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

        public FeedbackSimpleSplit()
        {
            _train = Container.Feedbacks.Where(f => f.Class == FeedbackClass.TRAIN);
            _test = Container.Feedbacks.Where(f => f.Class == FeedbackClass.TEST || f.Class == FeedbackClass.TEST_CANDIDATE);
        }

		// This constructor is an early-load split, that is, the container need to be loaded in advance
		public FeedbackSimpleSplit(float trainRatio)
        {
            var feedbacks = Container.Feedbacks.Shuffle();
            int trainCount = Convert.ToInt32(Container.Feedbacks.Count * trainRatio);
            
            _train = feedbacks.Take(trainCount);
            _test = feedbacks.Skip(trainCount);
        }

		public FeedbackSimpleSplit(IEnumerable<Feedback> train, IEnumerable<Feedback> test)
		{
			_train = train;
			_test = test;
		}
    }
}
