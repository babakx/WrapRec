using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class RatingSimpleSplitter : ISplitter<ItemRating>
    {
        public DataContainer Container { get; set; }

        public float TestPortion { get; set; }

        public RatingSimpleSplitter(DataContainer container, float testPortion)
        {
            Container = container;
            TestPortion = testPortion;

            var ratings = Container.Ratings.Shuffle();
            int trainCount = (int) Math.Round(ratings.Count() * (1 - testPortion));
            Train = ratings.Take(trainCount);
            Test = ratings.Skip(trainCount);
        }

        public IEnumerable<ItemRating> Train { get; private set; }

        public IEnumerable<ItemRating> Test { get; private set; }
        
    }
}
