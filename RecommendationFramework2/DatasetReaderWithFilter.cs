using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public abstract class DatasetReaderWithFilter<T> : IDatasetReader<T>
    {
        string _path;

        public List<IDataFilter<T>> Filters { get; private set; }

        public DatasetReaderWithFilter()
        {
            Filters = new List<IDataFilter<T>>();
        }

        public DatasetReaderWithFilter(string path) : this() 
        {
            _path = path;
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        

        public IEnumerable<T> ReadAll()
        {
            var result = ReadWithoutFiltering();

            Filters.ForEach(f =>
            {
                result = f.Filter(result);
            });

            return result;
        }

        public abstract IEnumerable<T> ReadWithoutFiltering();
    }
}
