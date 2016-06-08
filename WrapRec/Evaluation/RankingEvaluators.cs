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
	public class RankingEvaluators : Evaluator
	{
		public string CandidateItemsFile { get; private set; }
        public string CandidateUsersFile { get; set; }
        public CandidateItems CandidateItemsMode { get; private set; }
        public CandidateItems CandidateUsersMode { get; private set; }
        public int[] NumCandidates { get; private set; }
		public int[] CutOffs { get; private set; }

		int _maxNumCandidates;
		protected IList<string> _allCandidateItems;
        protected MultiKeyDictionary<int, int, StreamWriter> _perUserMetrics;

        public override void Setup()
		{
            // candidate items
            if (!SetupParameters.ContainsKey("candidateItemsMode"))
				CandidateItemsMode = CandidateItems.TRAINING;
			else
				CandidateItemsMode = (CandidateItems)Enum.Parse(typeof(CandidateItems), SetupParameters["candidateItemsMode"], true);

            if (SetupParameters.ContainsKey("candidateItemsFile"))
                CandidateItemsFile = SetupParameters["candidateItemsFile"];
            else if (CandidateItemsMode == CandidateItems.EXPLICIT)
                throw new WrapRecException("Expect a 'candidateItemsFile' for the mode 'explicit!'");

            // candidate useres
            if (!SetupParameters.ContainsKey("candidateUsersMode"))
                CandidateUsersMode = CandidateItems.TEST;
            else
                CandidateUsersMode = (CandidateItems)Enum.Parse(typeof(CandidateItems), SetupParameters["candidateUsersMode"], true);

            if (SetupParameters.ContainsKey("candidateUsersFile"))
                CandidateUsersFile = SetupParameters["candidateUsersFile"];
            else if (CandidateUsersMode == CandidateItems.EXPLICIT)
                throw new WrapRecException("Expect a 'candidateUsersFile' for the mode 'explicit!'");

            CutOffs = SetupParameters["cutOffs"].Split(',').Select(c => int.Parse(c)).ToArray();

			if (!SetupParameters.ContainsKey("numCandidates"))
				NumCandidates = new int[] { int.MaxValue };
			else
				NumCandidates = SetupParameters["numCandidates"].Split(',').Select(n =>
				{
					return (n == "max") ? int.MaxValue : int.Parse(n);
				}).ToArray();

			_maxNumCandidates = NumCandidates.Max();

            if (SetupParameters.ContainsKey("userMetricsFile"))
                _perUserMetrics = new MultiKeyDictionary<int, int, StreamWriter>();
        }

        public IEnumerable<User> GetCandidateUsers(Split split)
        {
            if (CandidateUsersMode == CandidateItems.TRAINING)
                return split.Train.Select(f => f.User).Distinct();
            else if (CandidateUsersMode == CandidateItems.TEST)
                return split.Test.Select(f => f.User).Distinct();
            else if (CandidateUsersMode == CandidateItems.UNION)
                return split.Container.Users.Values;
            else if (CandidateUsersMode == CandidateItems.OVERLAP)
            {
                var trainUsers = split.Train.Select(f => f.User).Distinct();
                var testUsers = split.Test.Select(f => f.User).Distinct();

                return trainUsers.Intersect(testUsers);
            }
            else // explicit
            {
                return File.ReadAllLines(CandidateUsersFile)
                    // if user exists return it, otherwise create new user
                    .Select(uId => split.Container.AddUser(uId));
            }

        }

        protected virtual void Initialize(Split split)
		{
			if (CandidateItemsMode == CandidateItems.TRAINING)
				_allCandidateItems = split.Train.Select(f => f.Item.Id).Distinct().ToList();
			else if (CandidateItemsMode == CandidateItems.TEST)
				_allCandidateItems = split.Test.Select(f => f.Item.Id).Distinct().ToList();
			else if (CandidateItemsMode == CandidateItems.UNION)
				_allCandidateItems = split.Container.Items.Values.Select(i => i.Id).ToList();
			else if (CandidateItemsMode == CandidateItems.OVERLAP)
			{
				var trainItems = split.Train.Select(f => f.Item.Id).Distinct();
				var testItems = split.Test.Select(f => f.Item.Id).Distinct();

				_allCandidateItems = trainItems.Intersect(testItems).ToList();
			}
			else // explicit
			{
				_allCandidateItems = File.ReadAllLines(CandidateItemsFile).ToList();
			}
		}

		public virtual IEnumerable<string> GetCandidateItems(Split split, User u)
		{
			var userItems = u.Feedbacks.Select(f => f.Item.Id);
			return _allCandidateItems.Except(userItems).Take(_maxNumCandidates);
		}

		public virtual IEnumerable<string> GetRelevantItems(Split split, User user)
		{
			return user.Feedbacks.Where(f => f.SliceType == FeedbackSlice.TEST && f.FeedbackType != FeedbackType.Negative)
				.Select(f => f.Item.Id).Distinct();
		}

		protected virtual IEnumerable<Tuple<string, float>> GetScoredRelevantItems(Model model, Split split, User user)
		{
			return GetRelevantItems(split, user).Select(i => new Tuple<string, float>(i, model.Predict(user.Id, i)));
		}

		protected virtual IEnumerable<Tuple<string, float>> GetScoredCandidateItems(Model model, Split split, User user)
		{
			return GetCandidateItems(split, user).Select(i => new Tuple<string, float>(i, model.Predict(user.Id, i)));
		}

		public override void Evaluate(EvaluationContext context, Model model, Split split)
		{
			split.UpdateFeedbackSlices();
			Initialize(split);

            var testUsers = GetCandidateUsers(split);
			int testedUsersCount = 0, testedCases = 0;
			var precision = new MultiKeyDictionary<int, int, double>();
			var recall = new MultiKeyDictionary<int, int, double>();
			var ndcg = new MultiKeyDictionary<int, int, double>();
			var mrrs = new MultiKeyDictionary<int, int, double>();
			var maps = new MultiKeyDictionary<int, int, double>();
			var distinctItems = new MultiKeyDictionary<int, int, List<string>>();


			// pre-compute IDCGs for speed up
			var idcgs = new Dictionary<int, double>();
			for (int k = 1; k <= CutOffs.Max(); k++)
				idcgs[k] = Enumerable.Range(1, k).Sum(i => 1.0 / Math.Log(i + 1, 2));

			// initialize measures
			foreach (int maxCand in NumCandidates)
			{
				foreach (int k in CutOffs)
				{
					precision[maxCand, k] = 0;
					recall[maxCand, k] = 0;
					ndcg[maxCand, k] = 0;
					mrrs[maxCand, k] = 0;
					maps[maxCand, k] = 0;
					distinctItems[maxCand, k] = new List<string>();

                    if (_perUserMetrics != null)
                    {
                        string path = SetupParameters["userMetricsFile"];
                        _perUserMetrics[maxCand, k] = new StreamWriter(
                            string.Format("{0}_{1}_{2}_{3}_{4}.{5}", path.GetPathWithoutExtension(), split.Id, model.Id, maxCand, k, path.GetFileExtension()));

                        _perUserMetrics[maxCand, k].WriteLine("UserId\tRecall");
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
				
				testedUsersCount++;
				testedCases += scoredRelevantItems.Count;

				// calculating measures for each numCandidates and cutoffs
				foreach (int maxCand in NumCandidates)
				{
					//var candidatesRankedList = scoredCandidateItems.Take(maxCand).OrderByDescending(i => i.Item2).ToList();
					var rankedList = scoredCandidateItems.Take(maxCand).Union(scoredRelevantItems).OrderByDescending(i => i.Item2).ToList();

					foreach (int k in CutOffs)
					{
						var topkItems = rankedList.Take(k).Select(ri => ri.Item1).ToList();

						// calculating diversity of recommendations in terms of number of distinct items
						distinctItems[maxCand, k] = distinctItems[maxCand, k].Union(topkItems).Distinct().ToList();

						// Calculate precision and recall with conventional method
						var hitsAtK = topkItems.Intersect(scoredRelevantItems.Select(i => i.Item1));

						int hitCount = 0;
						double dcg = 0;
						int lowestRank = int.MaxValue;
						double map = 0;

						foreach (string item in hitsAtK)
						{
							hitCount++;
							int rank =  topkItems.IndexOf(item);
							dcg += 1.0 / Math.Log(rank + 2, 2);
							map += (double)hitCount / (rank + 1);
							if (rank < lowestRank)
								lowestRank = rank;
						}

						int minRelevant = Math.Min(k, scoredRelevantItems.Count);

                        double rc = (double)hitCount / scoredRelevantItems.Count;

                        precision[maxCand, k] += (double)hitCount / k;
						recall[maxCand, k] += rc;
						ndcg[maxCand, k] += dcg / idcgs[minRelevant];
						maps[maxCand, k] += map / minRelevant;
                        //TODO: check to see if the following MRR definition is valid
                        mrrs[maxCand, k] += (lowestRank < int.MaxValue) ? 1.0 / (lowestRank + 1) : 0;

                        if (_perUserMetrics != null)
                            lock (this)
                                _perUserMetrics[maxCand, k].WriteLine("{0}\t{1:0.0000}", u.Id, rc);

                    }
                }
			});

			// aggregating measures and storing the results
			foreach (int maxCand in NumCandidates)
			{
				foreach (int k in CutOffs)
				{
					precision[maxCand, k] /= testedUsersCount;
					recall[maxCand, k] /= testedUsersCount;
					maps[maxCand, k] /= testedUsersCount;
					ndcg[maxCand, k] /= testedUsersCount;
					mrrs[maxCand, k] /= testedUsersCount;

					var results = new Dictionary<string, string>();
					results.Add("TestedCases", testedCases.ToString());
					results.Add("CandidatesMode", CandidateItemsMode.ToString());
					if (CandidateItemsMode == CandidateItems.EXPLICIT)
						results.Add("CandidatesFile", CandidateItemsFile.Substring(CandidateItemsFile.LastIndexOf('\\') + 1));
					results.Add("NumCandidates", maxCand == int.MaxValue ? "max" : maxCand.ToString());
					results.Add("AllCandidates", _allCandidateItems.Count.ToString());
					results.Add("CutOff", k.ToString());
					results.Add("Precision", string.Format("{0:0.0000}", precision[maxCand, k]));
					results.Add("Recall", string.Format("{0:0.0000}", recall[maxCand, k]));
					results.Add("MAP", string.Format("{0:0.0000}", maps[maxCand, k]));
					results.Add("MRR", string.Format("{0:0.0000}", mrrs[maxCand, k]));
					results.Add("NDCG", string.Format("{0:0.0000}", ndcg[maxCand, k]));
					results.Add("TotalRecomItems", distinctItems[maxCand, k].Count.ToString());
					results.Add("%Coverage", string.Format("{0:0.00}", 
						(100f * distinctItems[maxCand, k].Count / _allCandidateItems.Count)));
					results.Add("EvalMethod", GetEvaluatorName());

					context.AddResultsSet("rankingMeasures", results);

                    if (_perUserMetrics != null)
                        _perUserMetrics[maxCand, k].Close();
                }
            }
		}

		protected virtual string GetEvaluatorName()
		{
			return "UserBased";
		}
	}

}
