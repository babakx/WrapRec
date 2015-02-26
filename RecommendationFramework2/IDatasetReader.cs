using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec
{
    public interface IDatasetReader<out T>
    {
        IEnumerable<T> ReadAll();
    }

    public interface IDatasetReader
    {
        void LoadData(DataContainer container);
    }
}
