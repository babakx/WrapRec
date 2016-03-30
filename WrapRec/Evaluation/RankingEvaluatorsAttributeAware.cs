using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Models;

namespace WrapRec.Evaluation
{
	/// <summary>
	/// Ranking Evaluators for feedbacks with user or item attributes. This is not context aware evaluator
	/// Don't use this for feedbacks with 'feedback attributes (context)' since relevant items will get biased prediction
	/// because the candidate items are scored without any feedback attributes
	/// </summary>
	public class RankingEvaluatorsAttributeAware : RankingEvaluatorsOpr
	{
		protected override IEnumerable<Tuple<string, float>> GetScoredCandidateItems(Model model, Split split, User user)
		{
			return GetCandidateItems(split, user).Select(i => 
			{
				var item = split.Container.AddItem(i);
				var feedback = new Feedback(user, item);
				
				return new Tuple<string, float>(i, model.Predict(feedback));
			});
		}

		protected override IEnumerable<Tuple<string, float>> GetScoredRelevantItems(Model model, Split split, User user)
		{
			return user.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TEST)
				.Select(f => new Tuple<string, float>(f.Item.Id, model.Predict(f)));
		}

		protected override string GetEvaluatorName()
		{
			return "AttrBased";
		}
	}
}
