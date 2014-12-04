using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class CrossDomainSimpleSplitter : ISplitter<ItemRating>
    {
        public CrossDomainSimpleSplitter(CrossDomainDataContainer container)
        {
            Train = container.Ratings.Where(r => r.IsTest == false && r.Domain.IsTarget == true);
            Test = container.Ratings.Where(r => r.IsTest == true && r.Domain.IsTarget == true);
        }

        public CrossDomainSimpleSplitter(CrossDomainDataContainer container, float testPortion)
        {
            var targetRatings = container.Ratings.Where(r => r.Domain.IsTarget == true).Shuffle();
            int trainCount = (int)Math.Round(targetRatings.Count() * (1 - testPortion));

            Train = targetRatings.Take(trainCount);
            Test = targetRatings.Skip(trainCount);
        }
        
        public IEnumerable<ItemRating> Train { get; private set; }

        public IEnumerable<ItemRating> Test { get; private set; }
    }
}
