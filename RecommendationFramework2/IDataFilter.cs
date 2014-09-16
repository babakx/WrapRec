using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public interface IDataFilter<T>
    {
        IEnumerable<T> Filter(IEnumerable<T> source);
    }
}
