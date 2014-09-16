using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class Rating
    {
        [Key, Column(Order = 1)]
        public string UserId { get; set; }
        
        [Key, Column(Order = 2)]
        public string ItemId { get; set; }
        
        [Key, Column(Order = 3)]
        public int DatasetId { get; set; }
        
        public float Rate { get; set; }
        public float PredictedRate { get; set; }

        public RecordType? RecordType { get; set; } 

        public virtual User User { get; set; }
        public virtual Item Item { get; set; }
        public virtual Dataset Dataset { get; set; }
    }

    public enum RecordType
    {
        Any,
        Train,
        Test,
        Eval
    }
}
