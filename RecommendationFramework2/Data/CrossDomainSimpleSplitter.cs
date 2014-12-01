using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class CrossDomainSimpleSplitter : ISplitter<ItemRating>
    {
        public CrossDomainSimpleSplitter(CrossDomainDataContainer container)
        {
            Train = container.Ratings.Where(r => r.IsTest == false && r.Domain.IsTarget == true);
            Test = container.Ratings.Where(r => r.IsTest == true && r.Domain.IsTarget == true);
        }
        
        public IEnumerable<ItemRating> Train { get; private set; }

        public IEnumerable<ItemRating> Test { get; private set; }
    }
}
