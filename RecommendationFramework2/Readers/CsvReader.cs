using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LinqLib.Sequence;
using CsvHelper;
using CsvHelper.Configuration;

namespace RF2.Readers
{
    public class CsvReader<T> : DatasetReaderWithFilter<T>
    {
        CsvHelper.CsvReader _reader;

        public CsvReader(string path)
            : this(path, new CsvConfiguration())
        { }

        public CsvReader(string path, CsvConfiguration config)
            : base(path)
        {
            _reader = new CsvReader(File.OpenText(this.Path), config);
        }

        public CsvReader(string path, CsvClassMap<T> csvMap)
            : this(path)
        {
            _reader.Configuration.RegisterClassMap(csvMap);
        }

        public CsvReader(string path, CsvConfiguration config, CsvClassMap<T> csvMap)
            : this(path, config)
        {
            _reader.Configuration.RegisterClassMap(csvMap);
        }
       
        public override IEnumerable<T> ReadWithoutFiltering()
        {
            return _reader.GetRecords<T>().ToList();
        }
    }
}
