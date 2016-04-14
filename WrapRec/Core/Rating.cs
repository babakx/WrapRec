using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
    public class Rating : Feedback
    {
        public float Value { get; set; }

        public Rating(User user, Item item, float rating)
            : base(user, item)
        {
            Value = rating;
			this.FeedbackType = FeedbackType.Rating;
        }
    }
}
