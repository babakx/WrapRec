using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class Relation
    {
        public string UserId { get; set; }
        public string ConnectedId { get; set; }
        public ConnectionType? ConnectionType { get; set; }
        public float? ConnectionStrength { get; set; }
        public User User { get; set; }
        public User ConnectedUser { get; set; }
    }

    public enum ConnectionType
    { 
        Friend,
        Follower,
        Trust
    }
}
