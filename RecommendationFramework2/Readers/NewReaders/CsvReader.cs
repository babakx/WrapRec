using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.Readers.NewReaders
{
    public class CsvReader : IDatasetReader
    {
        public string Path { get; set; }

        public Domain Domain { get; set; }

        public bool IsTestReader { get; set; }

        CsvHelper.CsvReader _reader;

        public CsvReader(string path)
            : this(path, new CsvConfiguration())
        { }

        public CsvReader(string path, CsvConfiguration config)
            : this(path, config, null)
        { }

        public CsvReader(string path, CsvConfiguration config, bool isTestReader)
            : this(path, config, null, isTestReader)
        { }

        public CsvReader(string path, CsvConfiguration config, Domain domain)
            : this(path, config, domain, false)
        { }

        public CsvReader(string path, CsvConfiguration config, Domain domain, bool isTestReader)
        {
            Path = path;
            _reader = new CsvHelper.CsvReader(File.OpenText(this.Path), config);
            Domain = domain;
            IsTestReader = isTestReader;
        }

        public void LoadData(DataContainer container)
        {
            if (Domain != null)
            {
                ((CrossDomainDataContainer)container).CurrentDomain = Domain;
            }
            
            while (_reader.Read())
            {
                // userId, itemId, rating
                container.AddRating(_reader.GetField(0), _reader.GetField(1), float.Parse(_reader.GetField(2)), IsTestReader);
            }
        }
    }
}
