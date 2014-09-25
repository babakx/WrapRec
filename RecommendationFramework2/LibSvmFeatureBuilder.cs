using LibSvm;
using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class LibSvmFeatureBuilder
    {
        public Mapping Mapper { get; set; }

        public LibSvmFeatureBuilder()
        {
            Mapper = new Mapping();        
        }

        public virtual SvmNode[] GetSvmNode(ItemRating rating)
        {
            var svmNode = new SvmNode[2] {
                new SvmNode(Mapper.ToInternalID(rating.User.Id), 1),
                new SvmNode(Mapper.ToInternalID(rating.Item.Id + rating.Domain.Id), 1)
            };

            return svmNode;
        }
    }
}
