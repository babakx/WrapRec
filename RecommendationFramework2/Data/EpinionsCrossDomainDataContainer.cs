using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class EpinionsCrossDomainDataContainer : CrossDomainDataContainer
    {
        public int NumDomains { get; set; }

        List<int> _topCategories = new List<int>() { 21, 7, 18, 10, 32, 23, 12, 57 };

        public EpinionsCrossDomainDataContainer(int numDomains)
            : base()
        {
            NumDomains = numDomains;

            for (int i = 0; i < numDomains; i++)
            { 
                string dId = "ep" + i;
                Domains.Add(dId, new Domain(dId));
            }
        }

        public void PrintCategoryStatistics()
        {
            var counts = Ratings.Select(r => r.Item).GroupBy(i => i.GetProperty("Category"))
                .OrderByDescending(g => g.Count())
                .Select(g => string.Format("Category {0}, Count: {1}", g.Key, g.Count()));

            counts.ToList().ForEach(Console.WriteLine);
        }

        public override ItemRating AddRating(string userId, string itemId, float rating, bool isTest)
        {
            CurrentDomain = GetItemDomain(itemId);
            return base.AddRating(userId, itemId, rating, isTest);
        }


        public Domain GetItemDomain(string itemId)
        {
            int categoryId = int.Parse(Items[itemId].GetProperty("Category"));
            
            if (_topCategories.Contains(categoryId) && _topCategories.IndexOf(categoryId) < NumDomains - 1)
            {
                return Domains["ep" + (_topCategories.IndexOf(categoryId) + 1)];
            }
            
            // default domain is ep0
            return Domains["ep0"];
        }


    }
}
