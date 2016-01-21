using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
    public class Feedback : Entity
    {
        public User User { get; set; }
        public Item Item { get; set; }
        public FeedbackClass Class { get; set; }

        public Feedback(User user, Item item)
            : base()
        {
            User = user;
            Item = item;
            Class = FeedbackClass.NOT_SET;
        }


        public override string ToString()
        {
            return string.Format("{0}, UserId {1}, ItemId {2}", base.ToString(), User.Id, Item.Id);
        }
    }
}
