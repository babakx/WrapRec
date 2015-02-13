using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Utilities;

namespace WrapRec.Readers.NewReaders
{
    public class EpinionTrustReader : IDatasetReader
    {
        public IDatasetReader EpinionsReader { get; private set; }
        public string RelationsPath { get; set; }
        
        public EpinionTrustReader(IDatasetReader epinionsReader, string relationsPath)
        {
            EpinionsReader = epinionsReader;
            RelationsPath = relationsPath;
        }

        public void LoadData(DataContainer container)
        {
            // load standard rating data into container
            EpinionsReader.LoadData(container);

            // add relation specific data
            foreach (var line in File.ReadAllLines(RelationsPath))
            {
                var parts = line.Split(' ');
                string userId = parts[0];
                string connId = parts[1];

                container.Users[userId].AddProperty("Connections", connId);
            }

        }
    }
}
