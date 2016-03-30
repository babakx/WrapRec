using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
	public enum FeedbackType
	{
		Positive,
		Negative
	}

	public class Feedback : Entity
    {
        public User User { get; set; }
        public Item Item { get; set; }
        public FeedbackSlice SliceType { get; set; }
		public FeedbackType FeedbackType { get; set; }

        public Feedback(User user, Item item)
            : base()
        {
            User = user;
            Item = item;
            SliceType = FeedbackSlice.NOT_APPLICABLE;
			// By default a feedback is a positive Feedback
			FeedbackType = FeedbackType.Positive;
        }

		public IEnumerable<Attribute> GetAllAttributes()
		{
			return Attributes.Concat(User.Attributes).Concat(Item.Attributes);
		}

        public override string ToString()
        {
            return string.Format("{0}, UserId {1}, ItemId {2}", base.ToString(), User.Id, Item.Id);
        }
    }
}
