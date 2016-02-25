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
			_numValues += 2;
			return string.Format("{0}:1 {1}:1", userMappedId, itemMappedId);
		}
		
		public virtual string GetLibFmFeatureVector(Feedback feedback)
        {
            // "u" and "i" are added to make sure user and item Ids are distinguished
            int userMappedId = Mapper.ToInternalID(feedback.User.Id + "u");
            int itemMappedId = Mapper.ToInternalID(feedback.Item.Id + "i");
            
            string featVector;
            if (feedback is Rating)
                featVector = string.Format("{0} {1}:1 {2}:1", ((Rating)feedback).Value, userMappedId, itemMappedId);
            else
                featVector = string.Format("{0}:1 {1}:1", userMappedId, itemMappedId);

			_numValues += 2;

			foreach (string attr in FeedbackAttributes)
			{
				featVector += string.Format(" {0}:1", Mapper.ToInternalID(feedback.Attributes[attr]));
				_numValues++;
			}

			foreach (string attr in UserAttributes)
			{
				featVector += string.Format(" {0}:1", Mapper.ToInternalID(feedback.User.Attributes[attr]));
				_numValues++;
			}

			foreach (string attr in ItemAttributes)
			{
				featVector += string.Format(" {0}:1", Mapper.ToInternalID(feedback.Item.Attributes[attr]));
				_numValues++;
			}

            return featVector;
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
