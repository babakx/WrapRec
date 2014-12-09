using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WrapRec.Data;

namespace WrapRec.Readers.NewReaders
{
    public class MovieLensCrossDomainReader : IDatasetReader
    {
        public string MoviesPath { get; set; }
        public string RatingsPath { get; set; }
        
        public MovieLensCrossDomainReader(string moviesPath, string ratingsPath)
        {
            MoviesPath = moviesPath;
            RatingsPath = ratingsPath;
        }

        public void LoadData(DataContainer container)
        {
            if (!(container is MovieLensCrossDomainContainer))
                throw new WrapRecException("The data container should have type MovieLensCrossDomainContainer.");

            var mlContainer = (MovieLensCrossDomainContainer)container;

            Console.WriteLine("Reading movies...");

            foreach(string l in File.ReadAllLines(MoviesPath))
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                var item = mlContainer.AddItem(parts[0]);
                item.Properties["genres"] = parts[2];
            }

            Console.WriteLine("Creating domains...");
            mlContainer.CreateItemClusters();

            Console.WriteLine("Reading ratings...");
            foreach (string l in File.ReadAllLines(RatingsPath))
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                mlContainer.AddRating(parts[0], parts[1], float.Parse(parts[2]), false);
            }

        }
    }
}
