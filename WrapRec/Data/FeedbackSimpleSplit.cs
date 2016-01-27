using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;
using System.IO;

namespace WrapRec.Data.Splits
{
    public class FeedbackSimpleSplit : Split
    {

        public FeedbackSimpleSplit()
        {
			if (!Container.Feedbacks.Any(f => f.Class == FeedbackClass.TRAIN))
                throw new WrapRecException("Container does not contain any Train Feedbacks!");
            
            _train = Container.Feedbacks.Where(f => f.Class == FeedbackClass.TRAIN);
            _test = Container.Feedbacks.Where(f => f.Class == FeedbackClass.TEST || f.Class == FeedbackClass.TEST_CANDIDATE);
        }

        public FeedbackSimpleSplit(float trainRatio)
        {
            var feedbacks = Container.Feedbacks.Shuffle();
            int trainCount = Convert.ToInt32(Container.Feedbacks.Count * trainRatio);
            
            _train = feedbacks.Take(trainCount);
            _test = feedbacks.Skip(trainCount);
        }

    }
}
