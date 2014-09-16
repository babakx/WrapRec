using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WrapRec.Entities;

namespace WrapRec.Readers
{
    /*
    public class XDItemRatingReader : IDatasetReader<XDEnabledItemRating>
    {
        public Domain TargetDomain { get; set; }
        public Domain[] AuxDomains { get; set; }
        public IDatasetReader<ItemRating> TargetReader { get; set; }
        public IList<IDatasetReader<ItemRating>> AuxReaders { get; private set; }

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


        public XDItemRatingReader(IDatasetReader<ItemRating> targetReader, IList<IDatasetReader<ItemRating>> auxReaders)
        {
            TargetReader = targetReader;
            AuxReaders = auxReaders;
        }


        
        public IEnumerable<XDEnabledItemRating> ReadAll()
        {
            var targetRatings = TargetReader.ReadAll();

            foreach (var reader in AuxReaders)
            {
                // make a dictionary of rated items for each user in the aux domain
                var userRatedItems = reader.ReadAll().GroupBy(r => r.User.Id)
                    .ToDictionary(g => g.Key, g => g.Select(r => r.Item.Id).ToList());

                
                
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
     * */
}
