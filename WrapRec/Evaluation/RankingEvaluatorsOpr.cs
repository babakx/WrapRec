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

		protected MultiKeyDictionary<int, int, StreamWriter> _predictionWriter;

		public override void Setup()
		{
			base.Setup();

			if (SetupParameters.ContainsKey("predictionFile"))
				_predictionWriter = new MultiKeyDictionary<int, int, StreamWriter>();
		}

        public override IEnumerable<User> GetCandidateUsers(Split split)
        {
            return split.Test.Select(f => f.User).Distinct();
        }

        public override void Evaluate(EvaluationContext context, Model model, Split split)
		{
            split.UpdateFeedbackSlices();
            Initialize(split);

            var testUsers = GetCandidateUsers(split);
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
					
					if (_predictionWriter != null)
					{
						string path = SetupParameters["predictionFile"];
						_predictionWriter[maxCand, k] = new StreamWriter(
							string.Format("{0}_{1}_{2}_{3}_{4}.{5}", path.GetPathWithoutExtension(), split.Id, model.Id, maxCand, k, path.GetFileExtension()));

						_predictionWriter[maxCand, k].WriteLine("UserId\tItemId\tScore\tIsCorrect");
					}
				}
			}

			Parallel.ForEach(testUsers, u => 
            {
				// the followings are heavy processes, the results are stored in lists to prevent over computing
				var scoredRelevantItems = GetScoredRelevantItems(model, split, u).ToList();
                var scoredCandidateItems = GetScoredCandidateItems(model, split, u).ToList();

				if (scoredRelevantItems.Count == 0)
					return;

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
							bool correct = false;

							if (rank < k)
							{
								// if the relevant items falls into the top k items recall would be one (because the only relevent items is covered)
								// IDCG would be one as well (if relevant item appears in the first position)
								recallsOpr[maxCand, k] += 1;
								ndcgOpr[maxCand, k] += 1.0 / Math.Log(rank + 2, 2);
								mrrOpr[maxCand, k] += 1.0 / (rank + 1);
								correct = true;
							}

							if (_predictionWriter != null)
								lock (this)
									_predictionWriter[maxCand, k].WriteLine("{0}\t{1}\t{2}\t{3}", u.Id, item.Item1, item.Item2, correct ? 1 : 0);	
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

					if (_predictionWriter != null)
						_predictionWriter[maxCand, k].Close();
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
