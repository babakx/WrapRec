using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class PositiveFeedbackSimpleSplitter : ISplitter<PositiveFeedback>
    {
        public DataContainer Container { get; set; }

        public float TestPortion { get; set; }

        public PositiveFeedbackSimpleSplitter(DataContainer container, float testPortion)
        {
            Container = container;
            TestPortion = testPortion;

            var pf = Container.PositiveFeedbacks.Shuffle();
            int trainCount = (int) Math.Round(pf.Count() * (1 - testPortion));
            Train = pf.Take(trainCount);
            Test = pf.Skip(trainCount);
        }

        public IEnumerable<PositiveFeedback> Train { get; private set; }

        public IEnumerable<PositiveFeedback> Test { get; private set; }
    }
}
