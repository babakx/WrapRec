using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public interface ISplitter 
    {
        void Split(Dataset dataset, RecSysContext context, ref IQueryable<Rating> trainset, ref IQueryable<Rating> testset, ref IQueryable<Rating> evalset);
    }
}
