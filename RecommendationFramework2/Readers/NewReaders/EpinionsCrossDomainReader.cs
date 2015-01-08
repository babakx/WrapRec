using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.Readers.NewReaders
{
    public class EpinionsCrossDomainReader : IDatasetReader
    {
        public string FolderPath { get; set; }

        public EpinionsCrossDomainReader(string folderPath)
        {
            FolderPath = folderPath;
        }
        
        public void LoadData(DataContainer container)
        {
            if (!(container is EpinionsCrossDomainDataContainer))
                throw new WrapRecException("The data container should have type EpinionsCrossDomainDataContainer.");

            var ecdContainer = (EpinionsCrossDomainDataContainer) container;

            string reviewsPath = FolderPath + "\\Reviews.csv";

            // add products
            foreach (string line in File.ReadAllLines(FolderPath + "\\Products.csv").Skip(1))
            {
                var parts = line.Split(',');
                var item = ecdContainer.AddItem(parts[0]);
                item.Properties["Category"] = parts[1];
            }

            // add reviews
            foreach (string line in File.ReadAllLines(FolderPath + "\\Reviews.csv").Skip(1))
            {
                var parts = line.Split(',');
                ecdContainer.AddRating(parts[1], parts[4], float.Parse(parts[2]), false);
            }

            
        }
    }
}
