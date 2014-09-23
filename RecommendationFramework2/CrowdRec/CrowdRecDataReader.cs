using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WrapRec.CrowdRec
{
    public class CrowdRecDataReader : IDatasetReader
    {
        string _entitiesFile, _relationsFile;
        
        public CrowdRecDataReader(string entitesFile, string relationsFile)
        {
            _entitiesFile = entitesFile;
            _relationsFile = relationsFile;
        }
        
        public void LoadData(DataContainer container)
        {
            if (!(container is CrowdRecDataContainer))
            {
                throw new Exception("The data container should have type CrowdRecDataContainer.");
            }

            var crContainer = (CrowdRecDataContainer)container;

            LoadEntities(crContainer);
            LoadRelations(crContainer);
        }


        private void LoadEntities(CrowdRecDataContainer container)
        {
            Console.WriteLine("Importing entities...");

            foreach (var line in File.ReadAllLines(_entitiesFile))
            {
                var tokens = line.Split('\t');

                string entityType = tokens[0];
                string entityId = tokens[1];

                double temp;
                double? timestamp;

                if (double.TryParse(tokens[2], out temp))
                    timestamp = temp;
                else
                    timestamp = null;

                string properties = tokens.Length > 3 ? tokens[3] : "{}";

                if (entityType.ToLower() == "user")
                    container.Users.Add(entityId, container.CreateUser(entityId, timestamp, properties));
                else if (entityType.ToLower() == "movie")
                    container.Items.Add(entityId, container.CreateItem(entityId, timestamp, properties));
            }
        }

        private void LoadRelations(CrowdRecDataContainer container)
        {
            Console.WriteLine("Importing relations...");

            foreach (var line in File.ReadAllLines(_relationsFile))
            {
                var tokens = line.Split('\t');

                if (tokens.Length < 5)
                    throw new Exception("Expect 5 tab seperated fields.");
                
                string relationType = tokens[0];
                string relationId = tokens[1];

                double temp;
                double? timestamp;

                if (double.TryParse(tokens[2], out temp))
                    timestamp = temp;
                else
                    timestamp = null;

                ItemRating ir = container.CreateItemRating(relationId, timestamp, tokens[3], tokens[4]);

                container.Ratings.Add(ir);
            }
        }
    }
}
