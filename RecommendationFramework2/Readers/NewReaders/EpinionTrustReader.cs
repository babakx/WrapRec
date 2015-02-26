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
        public IDatasetReader[] EpinionsReaders { get; private set; }
        public string RelationsPath { get; set; }
        
        public EpinionTrustReader(IDatasetReader[] epinionsReaders, string relationsPath)
        {
            EpinionsReaders = epinionsReaders;
            RelationsPath = relationsPath;
        }

        public void LoadData(DataContainer container)
        {
            // load standard rating data into container
            foreach (var reader in EpinionsReaders)
            {
                reader.LoadData(container);
            }

            // add relation specific data
            foreach (var line in File.ReadAllLines(RelationsPath).Skip(1))
            {
                var parts = line.TrimStart(' ').Split('\t');
                string userId = parts[0] + "u";
                string connId = parts[1];
                string strngth = parts[2];

                if (container.Users.ContainsKey(userId))
                    container.Users[userId].AddProperty("Connections", connId + " " + parts[2]);
            }
        }
    }
}
