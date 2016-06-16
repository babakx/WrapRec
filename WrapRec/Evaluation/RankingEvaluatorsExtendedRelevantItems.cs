using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.IO;
using WrapRec.Models;
using LinqLib.Sequence;

namespace WrapRec.Evaluation
{
    /// <summary>
    /// If the ground truth items for a user is more than the one that appears in the test data you can use this class
    /// Note that, this class violates the decopouling of EvaluationContext and Split. You should make sure that the 
    /// right split is used when using this evaluator
    /// </summary>
    public class RankingEvaluatorsExtendedRelevantItems : RankingEvaluators
    {
        protected DataContainer RelevantsContainer;

        public override void Setup()
        {
            base.Setup();

            var readers = SetupParameters["relevantReaders"].Split(',').Select(rId => ExperimentManager.ParseDataReader(rId));
            RelevantsContainer = new DataContainer();

            Logger.Current.Info("Loading extra relevant items...");

            foreach (DatasetReader reader in readers)
                RelevantsContainer.DataReaders.Add(reader);

            RelevantsContainer.Load();
        }

        public override void Evaluate(EvaluationContext context, Model model, Split split)
        {
            var allItemIds = RelevantsContainer.Items.Keys.ToList();

            // only keep the items that are observed in the original container
            foreach (string itemId in allItemIds)
            {
                if (!split.Container.Items.ContainsKey(itemId))
                    RelevantsContainer.RemoveItem(RelevantsContainer.Items[itemId]);
            } 

            base.Evaluate(context, model, split);
        }

        public override IEnumerable<string> GetRelevantItems(Split split, User user)
        {
            var relevant = base.GetRelevantItems(split, user);
            if (RelevantsContainer.Users.ContainsKey(user.Id))
                relevant = relevant.Concat(RelevantsContainer.Users[user.Id].Feedbacks.Select(f => f.Item.Id));

            return relevant;
        }

        protected override string GetEvaluatorName()
        {
            return "OPR-ERI";
        }
    }
}
