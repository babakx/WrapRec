using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using System.IO;

namespace RecSysChallenge
{
    public class MovieTweetingsReader : IDatasetReader
    {
        private string _trainSet, _testSet;
        
        public MovieTweetingsReader(string trainingSet, string testSet)
        {
            _trainSet = trainingSet;
            _testSet = testSet;
        }
        
        public void LoadData(DataContainer container)
        {
            if (!(container is MovieTweetingsDataContainer))
            {
                throw new Exception("The data container should have type MovieTweetingsDataContainer.");
            }

            var mtContainer = (MovieTweetingsDataContainer)container;

            Console.WriteLine("Importing training set...");

            foreach (string l in File.ReadAllLines(_trainSet).Skip(1))
            {
                var tokens = l.Split(',');
                string tweetJson = tokens.Skip(4).Aggregate((a, b) => a + ',' + b);
                mtContainer.AddMovieTweeting(tokens[0], tokens[1], float.Parse(tokens[2]), tweetJson, false);
            }

            Console.WriteLine("Importing test set...");

            foreach (string l in File.ReadAllLines(_testSet).Skip(1))
            {
                var tokens = l.Split(',');
                string tweetJson = tokens.Skip(4).Aggregate((a, b) => a + ',' + b);
                mtContainer.AddMovieTweeting(tokens[0], tokens[1], float.Parse(tokens[2]), tweetJson, true);
            }
        }
    }
}
