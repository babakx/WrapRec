using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class FeedbackCrossValidation : Split
    {
        public FeedbackCrossValidation(int numFolds)
        {
            SubSplits = Enumerable.Range(1, numFolds)
                .Select(i => new FeedbackSimpleSplit(1f - (float)1f / numFolds) { Id = this.Id + "-fold" + i });
        }
    }
}
