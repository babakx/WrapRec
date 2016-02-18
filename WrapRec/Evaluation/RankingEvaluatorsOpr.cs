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
            InitializeCandidateItems(split);

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
				var candidateItems = GetCandidateItems(split, u);
			
				// the followings are heavy processes, the results are stored in lists to prevent over computing
				var scoredRelevantItems = GetRelevantItems(split, u).Select(i => new Tuple<string, double>(i, model.Predict(u.Id, i))).ToList();
                var scoredCandidateItems = candidateItems.Select(i => new Tuple<string, double>(i, model.Predict(u.Id, i))).ToList();

                testedCases += scoredRelevantItems.Count;

				// calculating measures for each numCandidates and cutoffs
				foreach (int maxCand in NumCandidates)
				{
					var candidatesRankedList = scoredCandidateItems.Take(maxCand).OrderByDescending(i => i.Item2).ToList();

                    // Calculate recall with One-plus-random method
                    foreach (Tuple<string, double> item in scoredRelevantItems)
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
					results.Add("NumCandidates", maxCand == int.MaxValue ? "max" : maxCand.ToString());
					results.Add("CutOff", k.ToString());
					results.Add("Recall", string.Format("{0:0.0000}", recallsOpr[maxCand, k]));
					results.Add("MRR", string.Format("{0:0.0000}", mrrOpr[maxCand, k]));
					results.Add("NDCG", string.Format("{0:0.0000}", ndcgOpr[maxCand, k]));
					results.Add("EvalMethod", "OPR");

					context.AddResultsSet("rankingMeasures", results);
				}				
			}
		}
    }

}
