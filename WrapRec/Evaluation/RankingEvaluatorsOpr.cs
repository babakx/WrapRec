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
using LinqLib.Sequence;

namespace WrapRec.Evaluation
{
	public class RankingEvaluatorsOpr : RankingEvaluators
	{

        public override void Evaluate(EvaluationContext context, Model model, Split split)
		{
            split.UpdateFeedbackSlices();
            Initialize(split);

            var testUsers = split.Test.Select(f => f.User).Distinct();
            int testedCases = 0;
            var recallsOpr = new MultiKeyDictionary<int, int, double>();
			var ndcgOpr = new MultiKeyDictionary<int, int, double>();
			var mrrOpr = new MultiKeyDictionary<int, int, double>();

			// initialize measures
			foreach (int maxCand in NumCandidates)
			{
				foreach (int k in CutOffs)
				{
                    recallsOpr[maxCand, k] = 0;
					ndcgOpr[maxCand, k] = 0;
					mrrOpr[maxCand, k] = 0;
				}
			}

			Parallel.ForEach(testUsers, u => 
            {
				// the followings are heavy processes, the results are stored in lists to prevent over computing
				var scoredRelevantItems = GetScoredRelevantItems(model, split, u).ToList();
                var scoredCandidateItems = GetScoredCandidateItems(model, split, u).ToList();

                testedCases += scoredRelevantItems.Count;

				// calculating measures for each numCandidates and cutoffs
				foreach (int maxCand in NumCandidates)
				{
					var candidatesRankedList = scoredCandidateItems.Take(maxCand).OrderByDescending(i => i.Item2).ToList();

                    // Calculate recall with One-plus-random method
                    foreach (Tuple<string, float> item in scoredRelevantItems)
                    {
                        int rank = IndexOfNewItem(candidatesRankedList, item.Item2);
						foreach (int k in CutOffs)
						{
							if (rank < k)
							{
								// if the relevant items falls into the top k items recall would be one (because the only relevent items is covered)
								// IDCG would be one as well (if relevant item appears in the first position)
								recallsOpr[maxCand, k] += 1;
								ndcgOpr[maxCand, k] += 1.0 / Math.Log(rank + 2, 2);
								mrrOpr[maxCand, k] += 1.0 / (rank + 1);
							}
						}
                    }
				}
            });

			// aggregating measures and storing the results
			foreach (int maxCand in NumCandidates)
			{
				foreach (int k in CutOffs)
				{
                    recallsOpr[maxCand, k] /= testedCases;
					ndcgOpr[maxCand, k] /= testedCases;
					mrrOpr[maxCand, k] /= testedCases;

                    var results = new Dictionary<string, string>();
                    results.Add("TestedCases", testedCases.ToString());
                    results.Add("CandidatesMode", CandidateItemsMode.ToString());
					if (CandidateItemsMode == CandidateItems.EXPLICIT)
						results.Add("CandidatesFile", CandidateItemsFile.Substring(CandidateItemsFile.LastIndexOf('\\') + 1));
					results.Add("NumCandidates", maxCand == int.MaxValue ? "max" : maxCand.ToString());
					results.Add("CutOff", k.ToString());
					results.Add("Recall", string.Format("{0:0.0000}", recallsOpr[maxCand, k]));
					results.Add("MRR", string.Format("{0:0.0000}", mrrOpr[maxCand, k]));
					results.Add("NDCG", string.Format("{0:0.0000}", ndcgOpr[maxCand, k]));
					results.Add("EvalMethod", GetEvaluatorName());

					context.AddResultsSet("rankingMeasures", results);
				}				
			}
		}

		protected int IndexOfNewItem(IList<Tuple<string, float>> descSortedList, double newItemScore)
		{
			int startIndex = 0, endIndex = descSortedList.Count - 1;

			while (startIndex + 1 < endIndex)
			{
				int index = (startIndex + endIndex) / 2;

				if (newItemScore == descSortedList[index].Item2)
					return index;
				else if (newItemScore < descSortedList[index].Item2)
					startIndex = index;
				else
					endIndex = index;
			}

			if (newItemScore >= descSortedList[startIndex].Item2)
				return startIndex;
			else if (newItemScore < descSortedList[endIndex].Item2)
				return endIndex + 1;
			else
				return endIndex;
		}

		protected override string GetEvaluatorName()
		{
			return "OPR";
		}
    }
}
