using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Evaluation
{
    public class ItemRankingEvaluationContext : EvalutationContext<ItemRanking>
    {
        IEnumerable<UserRankedList> _testUsersRankedList;

        public ItemRankingEvaluationContext(IModel model, IDataset<ItemRanking> dataset)
            : base(model, dataset)
        { }

        private void CreateTestUsersRankedList()
        {
            var model = (IPredictor<ItemRanking>)Model;

            if (!model.IsTrained)
                model.Train(Dataset.TrainSamples);

            Console.WriteLine("Testing on test set...");

            _testUsersRankedList = Dataset.TestSamples.GroupBy(ir => ir.User.Id).AsEnumerable().Select(g =>
            {
                var rankedList = ((IItemRecommender)Model).Recommend(new User(g.Key));
                rankedList.CorrectItems = g.Select(ir => ir.Item).Distinct().ToList();

                return rankedList;
            }).ToList();
        }

        public IEnumerable<UserRankedList> GetTestUsersRankedList()
        {
            if (_testUsersRankedList == null)
                CreateTestUsersRankedList();

            return _testUsersRankedList;
        }
    }
}
