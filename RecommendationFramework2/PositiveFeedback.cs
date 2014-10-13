using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class PositiveFeedback : UserItem
    {
        public float PredictedScore { get; set; }
        
        public PositiveFeedback(string userId, string itemId)
            : base(userId, itemId)
        { }

        public PositiveFeedback(User user, Item item)
            : base(user, item)
        { }


    }
}
