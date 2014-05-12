using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public interface IDataFilter<T>
    {
        IEnumerable<T> Filter(IEnumerable<T> source);
    }
}
