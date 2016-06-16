using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class AttributeBasedSplit : FeedbackSimpleSplit
    {
        public override void Setup()
        {
            base.Setup();
            
            // the following constrain only applies to train set
            var parts = SetupParameters["attrEqual"].Split(':');
            _train = _train.Where(f => f.Attributes[parts[0]].Value == parts[1]);
        }
    }
}
