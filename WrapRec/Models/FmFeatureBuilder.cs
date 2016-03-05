using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;

namespace WrapRec.Models
{
    public class FmFeatureBuilder 
    {
        public Mapping Mapper { get; set; }
        public IList<string> UserAttributes { get; set; }
        public IList<string> ItemAttributes { get; set; }
        public IList<string> FeedbackAttributes { get; set; }
		
		ulong _numValues = 0;
        
		public FmFeatureBuilder()
        {
            Mapper = new Mapping();
            UserAttributes = new List<string>();
            ItemAttributes = new List<string>();
            FeedbackAttributes = new List<string>();
        }

		public virtual string GetLibFmFeatureVector(string userId, string itemId)
		{
			// "u" and "i" are added to make sure user and item Ids are distinguished
			int userMappedId = Mapper.ToInternalID(userId + "u");
			int itemMappedId = Mapper.ToInternalID(itemId + "i");
			
			return string.Format("{0}:1 {1}:1", userMappedId, itemMappedId);
		}
		
		public virtual string GetLibFmFeatureVector(Feedback feedback)
        {
            string featVector;
			if (feedback is Rating)
				featVector = ((Rating)feedback).Value + " " + GetLibFmFeatureVector(feedback.User.Id, feedback.Item.Id);
			else
				featVector = GetLibFmFeatureVector(feedback.User.Id, feedback.Item.Id);

			_numValues += 2;

			var feedbackAttrs = feedback.Attributes.Where(a => FeedbackAttributes.Contains("all") || FeedbackAttributes.Contains(a.Name));
			var itemAttrs = feedback.Item.Attributes.Where(a => ItemAttributes.Contains("all") || ItemAttributes.Contains(a.Name));
			var userAttrs = feedback.User.Attributes.Where(a => UserAttributes.Contains("all") || UserAttributes.Contains(a.Name));

			foreach (var attr in feedbackAttrs.Union(userAttrs).Union(itemAttrs))
			{
				string trans = TranslateAttribute(attr);
				if (!string.IsNullOrEmpty(trans))
				{
					featVector += " " + trans;
					_numValues++;
				}
			}

            return featVector;
        }

		protected string TranslateAttribute(WrapRec.Core.Attribute attr)
		{
			string trans = "";
			
			if (attr.Type == AttributeType.Binary && (attr.Value == "1" || attr.Value == "true"))
				trans = string.Format("{0}:1", Mapper.ToInternalID(attr.Name));
			else if (attr.Type == AttributeType.Discrete)
				trans = string.Format("{0}:1", Mapper.ToInternalID(attr.Value));
			else if (attr.Type == AttributeType.RealValued)
				trans = string.Format("{0}:{1}", Mapper.ToInternalID(attr.Name), attr.Value);

			return trans;
		}

		public void RestartNumValues()
		{
			_numValues = 0;
		}

		public ulong GetNumMappedValues()
		{
			return _numValues;
		}
    }
}
