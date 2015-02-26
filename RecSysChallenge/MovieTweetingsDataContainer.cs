using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Data;

namespace RecSysChallenge
{
    public class MovieTweetingsDataContainer : DataContainer
    {
        public Dictionary<ItemRating, Tweet> Tweets { get; private set; }

        public MovieTweetingsDataContainer()
            : base()
        {
            Tweets = new Dictionary<ItemRating, Tweet>();
        }

        public void AddMovieTweeting(string userId, string movieId, float rating, string tweetJson, bool isTest)
        {
            var itemRating = base.AddRating(userId, movieId, rating, isTest);

            var tweet = Tweet.FromJson(tweetJson);
            tweet.ItemRating = itemRating;

            Tweets.Add(itemRating, tweet);
        }
    }
}
