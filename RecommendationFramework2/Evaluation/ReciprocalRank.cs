using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Evaluation
{
    public class ReciprocalRank : IEvaluator<ItemRanking>
    {
        public void Evaluate(EvalutationContext<ItemRanking> context)
        {
            if (!(context is ItemRankingEvaluationContext))
                throw new Exception("Wrong evaluation context.");

            var model = (IUserItemMapper)context.Model;

            double rr = ((ItemRankingEvaluationContext)context).GetTestUsersRankedList()
                .Select(url => MyMediaLite.Eval.Measures.ReciprocalRank.Compute(
                    url.GetMappedItemIds(model.ItemsMap),
                    url.GetMappedCorrectItemIds(model.ItemsMap)))
                .Average();

            context["ReciprocalRank"] = rr;

            Console.WriteLine(string.Format("ReciprocalRank: {0:0.0000}", rr));
        }
    }
}
