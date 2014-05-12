using CsvHelper.Configuration;
using RF2.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Samples.EntityDataReader;

namespace RF2.Data.DatasetImporters
{
    public class EpinionImporter : IDatasetImporter
    {
        string _datasetPath, _relationsPath;

        public EpinionImporter(string datasetPath, string relationsPath)
        {
            _datasetPath = datasetPath;
            _relationsPath = relationsPath;
        }
        
        public void ImportData(RecSysContext recSysContext, Dataset datasetRecord)
        {
            // step 1: importing item ratings (users, items and ratings)
            var conf = new CsvConfiguration();
            conf.Delimiter = " ";

            var epinionReader = new CsvReader<ItemRating>(_datasetPath, conf, new ItemRatingMap());
            recSysContext.ImportItemRatings(epinionReader.ReadSamples(), datasetRecord);

            // step 2: importing relations
            var relations = File.ReadAllLines(_relationsPath).ToCsvDictionary(' ')
                .Select(i => new Relation() { UserId = i["UserId"], ConnectedId = i["ConnectionId"], ConnectionType = ConnectionType.Trust });

            Console.WriteLine("Importing {0} relations ...", relations.Count());

            recSysContext.ImportToTable("Relations", relations);
        }

        public string GetDatasetName()
        {
            return "Epinion";
        }
    }
}
