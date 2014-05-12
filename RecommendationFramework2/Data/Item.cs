using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Data
{
    public class Item
    {
        [Key, Column(Order = 1)]
        public string Id { get; set; }
        
        [Key, ForeignKey("Dataset"), Column(Order = 2)]
        public int DatasetId { get; set; }

        public virtual Dataset Dataset { get; set; }
        
        public virtual ICollection<Rating> Ratings { get; set; }

        public Item()
        {
            Ratings = new List<Rating>();
        }
    }
}
