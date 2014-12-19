using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using System.IO;

namespace WrapRec
{
    public class CrossDomainLibFmFeatureBuilder : LibFmFeatureBuilder
    {
        /// <summary>
        /// Specifies the maximum number of ratings from each auxiliary domain that should be taken into account
        /// </summary>
        public int NumAuxRatings { get; set; }

        public Domain TargetDomain { get; set; }

        // cache the user extended vector into a dictionary to optimize the feature translation
        private Dictionary<string, string> _userCrossDomainFeatureVector;

        public CrossDomainLibFmFeatureBuilder(Domain targetDomain, int numAuxRating = 5)
            : base()
        {
            TargetDomain = targetDomain;
            _userCrossDomainFeatureVector = new Dictionary<string, string>();

            // default values for properties
            NumAuxRatings = numAuxRating;
        }


        public void LoadCachedUserData(string path)
        {
            foreach (var l in File.ReadAllLines(path))
            {
                var parts = l.Split(' ');
                string userId = parts[0];
                int numRatings = int.Parse(parts[1]);
                string extendedVector = "";

                for (int i = 0; i < Math.Min(numRatings, NumAuxRatings); i++)
                { 
                    var itemRating = parts[i + 2].Split(':');
                    extendedVector += string.Format(" {0}:{1:0.000000}", 
                        Mapper.ToInternalID(itemRating[0] + "m"), (double) int.Parse(itemRating[1]) / numRatings);
                }

                _userCrossDomainFeatureVector.Add(userId, extendedVector);
            }
        }

        public override string GetLibFmFeatureVector(ItemRating rating)
        { 
            // format:
            // rating uid:1 iid: 1 ai1: 0.5 ai2: 0.3 bi1: 0.1 bi2: 0.7

            string baseFeatVector = base.GetLibFmFeatureVector(rating);
            string userExtendedVector = "";

            if (!_userCrossDomainFeatureVector.TryGetValue(rating.User.Id, out userExtendedVector))
            {
                userExtendedVector = BuildUserCrossDomainFeatureVector(rating.User);
                _userCrossDomainFeatureVector.Add(rating.User.Id, userExtendedVector);
            }

            return baseFeatVector + userExtendedVector;
        }

        private string BuildUserCrossDomainFeatureVector(User user)
        {
            string extendedVector = "";

            Func<ItemRating, bool> checkActivation = r => r.Domain.IsActive == true; //r.IsActive == true;

            var domainRatings = user.Ratings.Where(checkActivation).GroupBy(r => r.Domain.Id);
            int userAuxDomains = user.Ratings.Select(r => r.Domain.Id).Distinct().Where(d => d != TargetDomain.Id).Count();

            int perDomainRatings;
            if (userAuxDomains > 0)
                perDomainRatings = (int)Math.Ceiling((double)NumAuxRatings / userAuxDomains);
            else
                perDomainRatings = 0;

            var avgUserRating = user.Ratings.Average(r => r.Rating);

            int take = perDomainRatings;
            int remain = NumAuxRatings;

            foreach (var d in domainRatings)
            {
                if (d.Key != TargetDomain.Id)
                { 
                    int ratingCount = d.Count();
                    string domainExtendedVector = "";

                    var ratings = d.OrderByDescending(r => r.Item.Ratings.Count).Take(take);

                    if (ratings.Count() > 0)
                    {
                        domainExtendedVector = ratings
                            .Select(dr => string.Format("{0}:{1:0.0000}", Mapper.ToInternalID(dr.Item.Id), (double)(dr.Rating - avgUserRating) / 4 + 1)) // dr.Rating / ratingCount))
                            .Aggregate((cur, next) => cur + " " + next);
                    }

                    if (!String.IsNullOrEmpty(domainExtendedVector.TrimEnd(' ')))
                        extendedVector += " " + domainExtendedVector;

                    remain = remain - perDomainRatings;
                    if (remain > 0)
                        take = perDomainRatings;
                    else
                        take = 0;
                }
            }
            
            return extendedVector;
        }

        // build feature vectors based on considering NumAuxRating ratings per domain

        //private string BuildUserCrossDomainFeatureVector(User user)
        //{
        //    string extendedVector = "";

        //    Func<ItemRating, bool> checkActivation = r => r.Domain.IsActive == true; //r.IsActive == true;

        //    var domainRatings = user.Ratings.Where(checkActivation).GroupBy(r => r.Domain.Id);
        //    var avgUserRating = user.Ratings.Average(r => r.Rating);

        //    foreach (var d in domainRatings)
        //    {
        //        if (d.Key != TargetDomain.Id)
        //        {
        //            int ratingCount = d.Count();

        //            string domainExtendedVector = d.OrderByDescending(r => r.Item.Ratings.Count)
        //                //d.Shuffle()
        //                .Take(NumAuxRatings)
        //                // ItemIds are concateneated with domain id to make sure that items in different domains are being distingushed
        //                //.Select(dr => string.Format("{0}:1", Mapper.ToInternalID(dr.Item.Id + d.Key.Id)))
        //                .Select(dr => string.Format("{0}:{1:0.0000}", Mapper.ToInternalID(dr.Item.Id), (double)(dr.Rating - avgUserRating) / 4 + 1)) // dr.Rating / ratingCount))
        //                .Aggregate((cur, next) => cur + " " + next);

        //            if (!String.IsNullOrEmpty(domainExtendedVector.TrimEnd(' ')))
        //                extendedVector += " " + domainExtendedVector;

        //        }
        //    }

        //    return extendedVector;
        //}

    }
}
