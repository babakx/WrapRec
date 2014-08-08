using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RF2.Entities;

namespace RF2.Readers
{
    public class XDItemRatingReader : IDatasetReader<XDEnabledItemRating>
    {
        public Domain TargetDomain { get; set; }
        public Domain[] AuxDomains { get; set; }

        public XDItemRatingReader(Domain targetDomain, Domain[] auxDomains)
        {
            TargetDomain = targetDomain;
            AuxDomains = auxDomains;
            
            // read ratings of target domain

            // find ratings of each user in the axu domains

            // extend the feature vector of users in target domain by aux ratings

            // write the format of the data according to the LibFm format

            // 
        }
        
        public IEnumerable<XDEnabledItemRating> ReadAll()
        {
            // this is not the best optimize way to read the ratings because a copy of ItemRating will be created in Memory
            var targetRatings = TargetDomain.RatingsReader.ReadSamples()
                .Select(ir => new XDEnabledItemRating(ir, TargetDomain))
                .ToList();

            foreach (var domain in AuxDomains)
            {
                var userRatings = GetUsersRatings(domain);

                targetRatings.GroupBy(tr => tr.User);
            }


            var auxRatings = AuxDomains.SelectMany(d => d.RatingsReader.ReadSamples()
                .Select(ir => new XDEnabledItemRating(ir, d)));



            //var userRatings = auxRatings.GroupBy(ir => ir.User);

            foreach (var xir in targetRatings)
            {
               // xir.User.ItemRatings = userRatings
            }
            return null;
        }

        private Dictionary<User, ICollection<ItemRating>> GetUsersRatings(Domain domain)
        {
            return null;
        }
    }
}
