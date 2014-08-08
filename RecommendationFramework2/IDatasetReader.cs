using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public interface IDatasetReader<out T>
    {
        IEnumerable<T> ReadAll();
    }
}
