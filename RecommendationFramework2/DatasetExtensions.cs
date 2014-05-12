using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public static class DatasetExtensions
    {
        public static int GetNumberOfUsers(this IDataset<UserItem> source)
        {
            return source.AllSamples.Select(ui => ui.User.Id).Distinct().Count(); 
        }

        public static int GetNumberOfItems(this IDataset<UserItem> source)
        {
            return source.AllSamples.Select(ui => ui.Item.Id).Distinct().Count();
        }
    }
}
