using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class User
    {
        [Key, Column(Order = 1)]
        public string Id { get; set; }

        [Key, ForeignKey("Dataset"), Column(Order = 2)]
        public int DatasetId { get; set; }

        public virtual Dataset Dataset {get; set;}

        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<Relation> ConnectedTo { get; set; }
        public virtual ICollection<Relation> IsConnectedBy { get; set; }

        public User()
        {
            Ratings = new List<Rating>();
            ConnectedTo = new List<Relation>();
            IsConnectedBy = new List<Relation>();
        }
    }
}
