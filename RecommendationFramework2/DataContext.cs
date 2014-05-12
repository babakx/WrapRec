using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class DataContext
    {
        public ICollection<User> Users { get; set; }
        public ICollection<Item> Items { get; set; }
        public ICollection<ItemRating> ItemRatings { get; set; }

        public DataContext()
        {
            Users = new List<User>();
            Items = new List<Item>();
            ItemRatings = new List<ItemRating>();
        }

        public DataContext(Domain[] domains)
        { 
            
        }
    }
}
