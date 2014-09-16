using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

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
        private Dictionary<User, string> _userCrossDomainFeatureVector;

        public CrossDomainLibFmFeatureBuilder(Domain targetDomain)
            : base()
        {
            TargetDomain = targetDomain;
            _userCrossDomainFeatureVector = new Dictionary<User, string>();

            // default values for properties
            NumAuxRatings = 5;
        }

        public override string GetLibFmFeatureVector(ItemRating rating)
        { 
            // format:
            // rating uid:1 iid: 1 ai1: 0.5 ai2: 0.3 bi1: 0.1 bi2: 0.7

            string userExtendedVector;

            if (!_userCrossDomainFeatureVector.TryGetValue(rating.User, out userExtendedVector))
            {
                userExtendedVector = BuildUserCrossDomainFeatureVector(rating.User);
                _userCrossDomainFeatureVector.Add(rating.User, userExtendedVector);
            }

            return base.GetLibFmFeatureVector(rating) + userExtendedVector;
        }

        private string BuildUserCrossDomainFeatureVector(User user)
        {
            string extendedVector = "";

            var domains = user.Ratings.GroupBy(r => r.Domain);

            foreach (var d in domains)
            {
                if (d.Key != TargetDomain)
                { 
                    int ratingCount = d.Count();
                    string domainExtendedVector = d.Shuffle()
                        .Take(NumAuxRatings)
                        // ItemIds are concateneated with domain id to make sure that items in different domains are being distingushed
                        //.Select(dr => string.Format("{0}:1", Mapper.ToInternalID(dr.Item.Id + d.Key.Id)))
                        .Select(dr => string.Format("{0}:{1:0.000}", Mapper.ToInternalID(dr.Item.Id + d.Key.Id), dr.Rating * 0.5 / ratingCount))
                        .Aggregate((cur, next) => cur + " " + next);

                    if (!String.IsNullOrEmpty(domainExtendedVector.TrimEnd(' ')))
                        extendedVector += " " + domainExtendedVector;

                }
            }

            return extendedVector;
        }

    }
}
