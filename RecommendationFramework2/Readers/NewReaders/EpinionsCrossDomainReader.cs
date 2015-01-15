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
        public string[] DomainFiles { get; set; }

        public EpinionsCrossDomainReader(string folderPath)
        {
            FolderPath = folderPath;
        }

        public EpinionsCrossDomainReader(string[] domainFiles)
        {
            DomainFiles = domainFiles;
        }

        private void NormalRead(EpinionsCrossDomainDataContainer container)
        {
            string reviewsPath = FolderPath + "\\Reviews.csv";

            // add products
            foreach (string line in File.ReadAllLines(FolderPath + "\\Products.csv").Skip(1))
            {
                var parts = line.Split(',');
                var item = container.AddItem(parts[0]);
                item.Properties["Category"] = parts[1];
            }

            // add reviews
            foreach (string line in File.ReadAllLines(FolderPath + "\\Reviews.csv").Skip(1))
            {
                var parts = line.Split(',');
                container.AddRating(parts[1], parts[4], float.Parse(parts[2]), false);
            }
        }

        private void DomainsLoad(EpinionsCrossDomainDataContainer container)
        {
            int i = 0;
            foreach (var file in DomainFiles)
            {
                container.CurrentDomain = container.Domains["ep" + i++];
                foreach (string line in File.ReadAllLines(file).Skip(1))
                {
                    var parts = line.Split(',');
                    container.AddRating(parts[0], parts[1], float.Parse(parts[2]), false);
                }
            }
        }

        public void LoadData(DataContainer container)
        {
            if (!(container is EpinionsCrossDomainDataContainer))
                throw new WrapRecException("The data container should have type EpinionsCrossDomainDataContainer.");

            var ecdContainer = (EpinionsCrossDomainDataContainer) container;

            if (string.IsNullOrEmpty(FolderPath))
                DomainsLoad(ecdContainer);
            else
                NormalRead(ecdContainer);
        }
    }
}
