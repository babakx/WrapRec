using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public interface ISplitter<T>
    {
        IList<T> Train { get; }

        IList<T> Test { get; }
    }
}
