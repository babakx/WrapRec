using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class ItemRanking : UserItem
    {
        public float PredictedRank { get; set; }

        public ItemRanking()
        { }
        
        public ItemRanking(string userId, string itemId)
            : base(userId, itemId)
        { }

        public ItemRanking(string userId, string itemId, float predictedScore)
            : this(userId, itemId)
        {
            PredictedRank = predictedScore;
        }
    }
}
