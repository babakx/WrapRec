using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite;
using MyMediaLite.Eval;

namespace WrapRec.Evaluation
{
    [Obsolete("Use MediaLitePositiveFeedback class instead")]
    public class MediaLiteItemRankingEvaluators : IEvaluator<ItemRanking>
    {
        IRecommender _recommender;

        public MediaLiteItemRankingEvaluators(IRecommender recommender)
        {
            _recommender = recommender;
        }
        
        public void Evaluate(EvalutationContext<ItemRanking> context)
        {
            if (!(context is ItemRankingEvaluationContext))
                throw new Exception("Wrong evaluation context.");

            var dataset = context.Dataset;
            var model = (IPredictor<ItemRanking>)context.Model;

            // make sure the model is trained
            if (!model.IsTrained)
                model.Train(dataset.TrainSamples);

            var mapper = (IUserItemMapper) context.Model;

            var testset = dataset.TestSamples.ToPosOnlyFeedback(mapper.UsersMap, mapper.ItemsMap);
            var trainset = dataset.TrainSamples.ToPosOnlyFeedback(mapper.UsersMap, mapper.ItemsMap);

            var results = _recommender.Evaluate(testset, trainset);

            foreach (var item in results)
            {
                context[item.Key] = item.Value;
                Console.WriteLine(string.Format("{0}: {1:0.0000}", item.Key, item.Value));
            }
            
            // calculate F1@5 and F1@10
            var precAt5 = (float)context["prec@5"];
            var precAt10 = (float)context["prec@5"];
            var recallAt5 = (float)context["recall@5"];
            var recallAt10 = (float)context["recall@10"];

            var f1At5 = precAt5 * recallAt5 * 2 / (precAt5 + recallAt5);
            var f1At10 = precAt10 * recallAt10 * 2 / (precAt10 + recallAt10);

            Console.WriteLine(string.Format("F1@5: {0:0.0000}", f1At5));
            Console.WriteLine(string.Format("F1@10: {0:0.0000}", f1At10));
        }
    }
}
