using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class Relation
    {
        //[Key, Column(Order = 1)]
        public string UserId { get; set; }

        //[Key, Column(Order = 2)]
        public string ConnectedId { get; set; }
        
        //[Key, Column(Order = 3)]
        public int DatasetId { get; set; }
        
        public ConnectionType? ConnectionType { get; set; }
        public float? ConnectionStrength { get; set; }

        public User User { get; set; }
        public User ConnectedUser { get; set; }
        public Dataset Dataset { get; set; }
    }

    public enum ConnectionType
    { 
        Friend,
        Follower,
        Trust
    }
}
