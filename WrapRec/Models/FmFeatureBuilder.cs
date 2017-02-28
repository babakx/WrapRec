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
				featVector = (feedback.FeedbackType == FeedbackType.Positive ? "1 " : "-1 ") + GetLibFmFeatureVector(feedback.User.Id, feedback.Item.Id);

			_numValues += 2;

			var feedbackAttrs = feedback.Attributes.Values.Where(a => FeedbackAttributes.Contains("all") || FeedbackAttributes.Contains(a.Name));
			var itemAttrs = feedback.Item.Attributes.Values.Where(a => ItemAttributes.Contains("all") || ItemAttributes.Contains(a.Name));
			var userAttrs = feedback.User.Attributes.Values.Where(a => UserAttributes.Contains("all") || UserAttributes.Contains(a.Name));

			foreach (var attr in feedbackAttrs.Union(userAttrs).Union(itemAttrs))
			{
				var feat = TranslateAttribute(attr);
				if (feat != null)
				{
					featVector += string.Format(" {0}:{1}", feat.Item1, feat.Item2);
					_numValues++;
				}
			}

            return featVector;
        }

		public Tuple<int, float> TranslateAttribute(WrapRec.Core.Attribute attr)
		{
			if (attr.Type == AttributeType.Binary && (attr.Value == "1" || attr.Value == "true"))
				return new Tuple<int, float>(Mapper.ToInternalID(attr.Name), 0.1f);
			else if (attr.Type == AttributeType.Discrete)
				return new Tuple<int, float>(Mapper.ToInternalID(attr.Value), 0.1f);
			else if (attr.Type == AttributeType.RealValued)
				return new Tuple<int,float>(Mapper.ToInternalID(attr.Name), float.Parse(attr.Value) * 0.1f);

			return new Tuple<int, float>(Mapper.ToInternalID(attr.Name), 0);
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
