using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RecSysChallenge
{
    public class Tweet
    {
        public ItemRating ItemRating { get; set; }

        public string Id { get; set; }

        public string TwitterUserId { get; set; }

        public int RetweetCount { get; set; }

        public int FavoriteCount { get; set; }

        public int FollowersCount { get; set; }

        public string MovieUrl { get; set; }

        public int FriendsCount { get; set; }

        public static Tweet FromJson(string tweetJson)
        {
            var jObject = JObject.Parse(tweetJson);

            var t = new Tweet();
            t.Id = jObject["id"].ToString();
            t.RetweetCount = (int)jObject["retweet_count"];
            t.FavoriteCount = (int)jObject["favorite_count"];
            t.FriendsCount = (int)jObject["user"]["friends_count"];
            t.FollowersCount = (int)jObject["user"]["followers_count"];
            t.TwitterUserId = jObject["user"]["id"].ToString();

            if (jObject["entities"]["urls"].Children().Count() > 0)
            {
                t.MovieUrl = jObject["entities"]["urls"][0]["expanded_url"].ToString();
            }
            else
            {
                t.MovieUrl = "unknown";
            }
            
            return t;
        }
    }
}
