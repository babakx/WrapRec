using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Utilities;

namespace WrapRec.Data
{
    public class CrossDomainDataContainer : DataContainer
    {
        public Dictionary<string, Domain> Domains { get; private set; }

        private Domain _currentDomain;

        private static Domain _defaultDomain;

        /// <summary>
        /// This is to specify the domain to which the data is being loaded
        /// </summary>
        public Domain CurrentDomain 
        {
            get
            {
                return _currentDomain;
            }
            set
            {
                if (!Domains.ContainsKey(value.Id))
                {
                    AddDomain(value);
                    _currentDomain = value;
                }
                else
                {
                    _currentDomain = Domains[value.Id];
                }
            }
        }

        public CrossDomainDataContainer()
            : base()
        {
            Domains = new Dictionary<string, Domain>();
        }

        /// <summary>
        /// This method return the default domain for single domain scenarios (this is necessary because the ratings for each user is added to a domain)
        /// (should be tested)
        /// </summary>
        /// <returns></returns>
        public static Domain GetDefualtDomain()
        {
            if (_defaultDomain == null)
                _defaultDomain = new Domain("default");

            return _defaultDomain;
        }
        
        public void AddDomain(Domain domain)
        {
            Domains.Add(domain.Id, domain);
        }

        public override ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            if (CurrentDomain == null)
            {
                throw new Exception(string.Format("Active dmain is not defined in the CrossDomainDataContainer"));
            }

            // ItemId is added with domainId to make sure that items in different domains have different ids
            //var ir = base.AddRating(userId, itemId + CurrentDomain.Id, rating, isTest);
            var ir = base.AddRating(userId, itemId, rating, isTest);

            ir.Domain = CurrentDomain;
            CurrentDomain.Ratings.Add(ir);

            return ir;
        }

        public Domain SpecifyTargetDomain(string domainId)
        {
            Domain target = null;

            foreach (var d in Domains.Values)
            {
                if (d.Id == domainId)
                {
                    d.IsTarget = true;
                    target = d;
                }
                else
                    d.IsTarget = false;
            }

            return target;
        }

        public Domain GetTargetDomain()
        {
            return Domains.Values.Where(d => d.IsTarget == true).SingleOrDefault();
        }

        public void DeactiveDomains(params string[] domainIds)
        {
            foreach (var d in Domains.Values.Where(d => domainIds.Contains(d.Id)))
            {
                d.IsActive = false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Domains, {1} Users, {2} Items, {3} Ratings", Domains.Count, Users.Count, Items.Count, Ratings.Count);
        }

        public void PrintStatistics()
        {
            var targetUserIds = Domains.Values.Where(d => d.IsTarget == true).Single().Ratings.Select(r => r.User.Id).Distinct().ToList();

            Console.WriteLine("Data Container Statistics: \n{0} \n\nDomains:", ToString());

            foreach (var d in Domains.Values)
            {
                Console.WriteLine(d.ToString());
                if (!d.IsTarget)
                {
                    var domainUserIds = d.Ratings.Select(r => r.User.Id).Distinct().ToList();
                    int numIntersectUsers = domainUserIds.Intersect(targetUserIds).Count();
                    Console.WriteLine("Num users in target domain: {0}\n", numIntersectUsers);
                }
            }
            
            Console.WriteLine("Data statistics: \nNum Test Samples: {0}\n", Ratings.Where(r => r.IsTest == true).Count());

            //var t = Ratings.Select(ir => long.Parse(ir.GetProperty("timestamp")));
            //Console.WriteLine("Min date: {0}, Max date: {1}", t.Min().FromUnixEpoch(), t.Max().FromUnixEpoch());

        }

        public void WriteHistogram(string outputFolder)
        {
            foreach (var domain in Domains.Values)
            {
                domain.WriteHistogram(string.Format("{0}\\{1}.usershist.csv", outputFolder, domain.Id));
            }
        }
    }
}
