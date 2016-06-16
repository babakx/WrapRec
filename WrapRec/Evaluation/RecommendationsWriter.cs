using MyMediaLite.Eval;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Models;
using WrapRec.Utils;

namespace WrapRec.Evaluation
{
    public class RecommendationsWriter : RankingEvaluators
    {
        public string OutputFile { get; private set; }

        public override void Setup()
        {
            base.Setup();

            OutputFile = SetupParameters["outputFile"];
        }

        public override void Evaluate(EvaluationContext context, Model model, Split split)
        {
            split.UpdateFeedbackSlices();
            Initialize(split);

            // if mode is explicit, make sure all item Ids are added to the ItemsMap dic
            if (CandidateItemsMode == CandidateItems.EXPLICIT && model is MmlRecommender)
                foreach (string itemId in _allCandidateItems)
                    ((MmlRecommender)model).ItemsMap.ToInternalID(itemId);

            var candidateUsers = GetCandidateUsers(split);
            var output = new List<string>();
            int maxCutOff = CutOffs.Max();

            Parallel.ForEach(candidateUsers, u =>
            {
                var scoredCandidateItems = GetScoredCandidateItems(model, split, u);

                // for this evaluator only max of NumCandidate and CutOff is considered
                var rankedList = scoredCandidateItems.OrderByDescending(i => i.Item2).Take(maxCutOff);

                string line = u.Id + " " + rankedList.Select(r => string.Format("{0}:{1:0.0000}", r.Item1, r.Item2))
                    .Aggregate((a, b) => a + " " + b);

                output.Add(line);
            });

            OutputFile = string.Format("{0}_{1}_{2}.{3}", OutputFile.GetPathWithoutExtension(), split.Id, model.Id, OutputFile.GetFileExtension());
            File.WriteAllLines(OutputFile, output);

            var results = new Dictionary<string, string>();

            results.Add("CandidatesMode", CandidateItemsMode.ToString());
            if (CandidateItemsMode == CandidateItems.EXPLICIT)
                results.Add("CandidatesFile", CandidateItemsFile.Substring(CandidateItemsFile.LastIndexOf('\\') + 1));
            results.Add("NumCandidates", NumCandidates.Max().ToString());
            results.Add("CutOff", maxCutOff.ToString());
            results.Add("OutputFile", OutputFile);
            results.Add("EvalMethod", GetEvaluatorName());

            context.AddResultsSet("rankingMeasures", results);
        }

        protected override string GetEvaluatorName()
        {
            return "UserBasedWriter";
        }
    }
}
