using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WrapRec.Data;

namespace WrapRec.Readers.NewReaders
{
    public class LibFmReader : IDatasetReader
    {
        public string TrainFile { get; set; }

        public string TestFile { get; set; }

        public Domain MainDomain { get; set; }

        public Domain AuxDomain { get; set; }

        public string UserDataPath { get; set; }

        public LibFmReader(string trainFile, string testFile)
        {
            TrainFile = trainFile;
            TestFile = testFile;
        }

        public void LoadData(DataContainer container)
        {
            var cdContainer = (CrossDomainDataContainer)container;

            Dictionary<string, int> numAuxUserRatings = File.ReadAllLines(UserDataPath)
                .Select(l =>
                {
                    var parts = l.Split(' ');
                    return new { UserId = parts[0], Count = int.Parse(parts[1]) };
                })
                .ToDictionary(d => d.UserId, d => d.Count);

            foreach (var l in File.ReadAllLines(TrainFile))
            {
                var parts = l.Split(' ');
                float rating = float.Parse(parts[0]);
                string userId = parts[1].Split(':')[0];
                string itemId = parts[2].Split(':')[0];

                cdContainer.ActiveDomain = MainDomain;
                cdContainer.AddRating(userId, itemId, rating, false);

                cdContainer.ActiveDomain = AuxDomain;

                for (int i = 3; i < parts.Length; i++)
                { 
                    var itemRating = parts[i].Split(':');
                    cdContainer.AddRating(userId, itemRating[0], float.Parse(itemRating[1]) * 10, false);
                }
            }

            foreach (var l in File.ReadAllLines(TestFile))
            {
                var parts = l.Split(' ');
                float rating = float.Parse(parts[0]);
                string userId = parts[1].Split(':')[0];
                string itemId = parts[2].Split(':')[0];

                cdContainer.ActiveDomain = MainDomain;
                cdContainer.AddRating(userId, itemId, rating, true);

                cdContainer.ActiveDomain = AuxDomain;

                for (int i = 3; i < parts.Length; i++)
                {
                    var itemRating = parts[i].Split(':');
                    cdContainer.AddRating(userId, itemRating[0], float.Parse(itemRating[1]), false);
                }
            }
        }
    }
}
