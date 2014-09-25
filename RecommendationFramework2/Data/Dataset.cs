using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class Dataset
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Relation> Relations { get; set; }

        public Dataset()
        {
            Ratings = new List<Rating>();
            Users = new List<User>();
            Items = new List<Item>();
            Relations = new List<Relation>();
        }

    }
}
