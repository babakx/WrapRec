using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;

namespace WrapRec.Evaluation
{
	
	public class RatingBasedRankingEvaluators : RankingEvaluators
	{
		Dictionary<User, float> _averageRating;
		float _averageAllUsers;

		protected override void Initialize(Split split)
		{
			base.Initialize(split);
			_averageRating = split.Train.GroupBy(f => f.User)
				.ToDictionary(g => g.Key, g => g.Average(f => ((Rating)f).Value));
			_averageAllUsers = _averageRating.Values.Average();
		}
		
		public override IEnumerable<string> GetRelevantItems(Split split, User user)
		{
			float threshold = _averageRating.ContainsKey(user) ? _averageRating[user] : _averageAllUsers;
			return user.Feedbacks //.Where(f => f.SliceType == FeedbackSlice.TEST) // && ((Rating)f).Value > threshold)
				.Select(f => f.Item.Id).Distinct();
		}

		protected override string GetEvaluatorName()
		{
			return "RBR";
		}
	}
}
