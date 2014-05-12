using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Eval.Measures;
using LinqLib.Operators;

namespace RF2.Evaluation
{
    public class NDCG : IEvaluator<ItemRanking>
    {
        public void Evaluate(EvalutationContext<ItemRanking> context)
        {
            if (!(context is ItemRankingEvaluationContext))
                throw new Exception("Wrong evaluation context.");

            var model = (IUserItemMapper) context.Model;
            
            double ndcg = ((ItemRankingEvaluationContext)context).GetTestUsersRankedList()
                .Select(url => MyMediaLite.Eval.Measures.NDCG.Compute(
                    url.GetMappedItemIds(model.ItemsMap), 
                    url.GetMappedCorrectItemIds(model.ItemsMap)))
                .Average();

            context["NDCG"] = ndcg;

            Console.WriteLine(string.Format("NDCG: {0:0.0000}", ndcg));
        }
    }
}
