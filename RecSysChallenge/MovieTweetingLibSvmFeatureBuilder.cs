using LibSvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;

namespace RecSysChallenge
{
    public class MovieTweetingLibSvmFeatureBuilder : LibSvmFeatureBuilder
    {
        MovieTweetingsDataContainer _container;
        int _maxFollowerCount, _maxFriendCount;
        public MovieTweetingLibSvmFeatureBuilder(MovieTweetingsDataContainer container)
            : base()
        {
            _container = container;
            _maxFollowerCount = _container.Tweets.Values.Max(t => t.FollowersCount);
            _maxFriendCount = _container.Tweets.Values.Max(t => t.FriendsCount);
        }
        
        public override LibSvm.SvmNode[] GetSvmNode(ItemRating rating)
        {
            int followersCount = _container.Tweets[rating].FollowersCount;
            double followersFeature = followersCount > 1000 ? 1.0 : (double)followersCount / 1000;

            int friendsCount = _container.Tweets[rating].FriendsCount;
            double friendsFeature = friendsCount > 1000 ? 1.0 : (double)friendsCount / 1000;
            
            var svmNode = new SvmNode[6] {
                //new SvmNode(Mapper.ToInternalID("RetweetCount"), _container.Tweets[rating].RetweetCount),
                new SvmNode(Mapper.ToInternalID("Rating"), rating.Rating),
                new SvmNode(Mapper.ToInternalID("FollowersCount"),  followersFeature),
                new SvmNode(Mapper.ToInternalID("FriendsCount"),  friendsFeature),
                new SvmNode(Mapper.ToInternalID(rating.User.Id), 1),
                new SvmNode(Mapper.ToInternalID(rating.Item.Id + rating.Domain.Id), 1),
                new SvmNode(Mapper.ToInternalID(_container.Tweets[rating].MovieUrl), 1),
            };

            return svmNode;
        }
    }
}
