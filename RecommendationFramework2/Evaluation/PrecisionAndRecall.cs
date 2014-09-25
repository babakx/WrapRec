using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Eval.Measures;
using LinqLib.Operators;
using MML = MyMediaLite.Eval.Measures;

namespace WrapRec.Evaluation
{
    public class PrecisionAndRecall : IEvaluator<ItemRanking>
    {
        int _position;

        public PrecisionAndRecall(int position)
        {
            _position = position;
        }
        
        public void Evaluate(EvalutationContext<ItemRanking> context)
        {
            if (!(context is ItemRankingEvaluationContext))
                throw new Exception("Wrong evaluation context.");

            var model = (IUserItemMapper)context.Model;

            var measures = ((ItemRankingEvaluationContext)context).GetTestUsersRankedList()
                .Select(url =>
                    {
                        var rankedItems = url.GetMappedItemIds(model.ItemsMap);
                        var correctItems = url.GetMappedCorrectItemIds(model.ItemsMap);

                        double ap = MML.PrecisionAndRecall.AP(rankedItems, correctItems);
                        double precAtN = MML.PrecisionAndRecall.PrecisionAt(rankedItems, correctItems, _position);
                        double recallAtN = MML.PrecisionAndRecall.RecallAt(rankedItems, correctItems, _position);

                        return new { AP = ap, PrecAtN = precAtN, RecallAtN = recallAtN };
                    }).ToList();

            if (!context.Items.ContainsKey("AP"))
            {
                context["AP"] = measures.Select(m => m.AP).Average();
                Console.WriteLine(string.Format("AP: {0:0.0000}", context["AP"]));
            }

            context["PrecAt" + _position] = measures.Select(m => m.PrecAtN).Average();
            context["RecallAt" + _position] = measures.Select(m => m.RecallAtN).Average();

            Console.WriteLine(string.Format("PrecAt {0}: {1:0.0000}", _position, context["PrecAt" + _position]));
            Console.WriteLine(string.Format("RecallAt {0}: {1:0.0000}", _position, context["RecallAt" + _position]));
        }
    }
}
