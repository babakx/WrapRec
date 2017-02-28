using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Models;
using System.IO;
using WrapRec.Utils;

namespace WrapRec.Evaluation
{
    public class RankingEvaluatorFromFile : RankingEvaluators
    {
        public string PredFile { get; set; }
        public string FeedbackFile { get; set; }
        public int NumItems { get; set; }
        public string TrainFile { get; set; }

        private List<Tuple<int, int>> _userItemsInTest;
        private List<Tuple<int, int>> _userItemsInTrain;
        private Dictionary<int, List<int>> _userCandidates;

        private Dictionary<int, List<int>> _userPosFeedback;
        private Dictionary<int, List<int>> _userTestFeedback;

        private MultiKeyDictionary<int, int, double> _scores;

        public override void Setup()
        {
            PredFile = SetupParameters["predFile"];
            FeedbackFile = SetupParameters["feedbackFile"];
            TrainFile = SetupParameters["trainFile"];
            //NumItems = int.Parse(SetupParameters["numItems"]);

            base.Setup();
        }

        protected override void Initialize(Split split)
        {
            var fr = new StreamReader(FeedbackFile);
            var pr = new StreamReader(PredFile);
            var tr = new StreamReader(TrainFile);

            _userItemsInTest = new List<Tuple<int, int>>();
            _userItemsInTrain = new List<Tuple<int, int>>();
            _scores = new MultiKeyDictionary<int, int, double>();
            _userTestFeedback = new Dictionary<int, List<int>>();
            _userCandidates = new Dictionary<int, List<int>>();

            string fline, pline;
            while ((fline = fr.ReadLine()) != null)
            {
                pline = pr.ReadLine();

                var fparts = fline.Split(',');

                int uId = int.Parse(fparts[0]);
                int iId = int.Parse(fparts[1]);

                if (fparts.Last() == "1")
                    _userItemsInTest.Add(new Tuple<int, int>(uId, iId));
                else
                {
                    if (!_userCandidates.ContainsKey(uId))
                        _userCandidates[uId] = new List<int>();

                    _userCandidates[uId].Add(iId);
                }
                _scores.Add(uId, iId, double.Parse(pline));
            }

            fr.Close();
            pr.Close();

            string tline;

            while ((tline = tr.ReadLine()) != null)
            {
                var tparts = tline.Split(',');

                int uId = int.Parse(tparts[0]);
                int iId = int.Parse(tparts[1]);

                if (tparts.Last() == "1")
                    _userItemsInTrain.Add(new Tuple<int, int>(uId, iId));

                //_scores.Add(uId, iId, 1f);
            }

            tr.Close();

            _userPosFeedback = _userItemsInTest.Union(_userItemsInTrain).GroupBy(ui => ui.Item1).ToDictionary(g => g.Key, g => g.Select(ui => ui.Item2).ToList());
            _allCandidateItems = _scores.SelectMany(kv => kv.Value.Keys).Select(i => i.ToString()).ToList();
            _userTestFeedback = _userItemsInTest.GroupBy(ui => ui.Item1)
                .ToDictionary(g => g.Key, g => g.Select(ui => ui.Item2).ToList());

            foreach (var kv in _userCandidates)
            {
                //if (kv.Value.Intersect(_userPosFeedback[kv.Key]).Any())
                //    throw new Exception("Should not be common items in both candidate and pos feedback!");
            }
        }


        public override IEnumerable<User> GetCandidateUsers(Split split)
        {
            return _userItemsInTest.Select(ui => ui.Item1).Distinct().Select(u => new User(u.ToString()));
        }

        public override IEnumerable<string> GetCandidateItems(Split split, User u)
        {
            //return _allCandidateItems.Select(i => int.Parse(i)).Except(_userPosFeedback[int.Parse(u.Id)]).Take(_maxNumCandidates)
            return _userCandidates[int.Parse(u.Id)]
                .Select(i => i.ToString()).Take(1000);
        }

        public override IEnumerable<string> GetRelevantItems(Split split, User user)
        {
            //return _userPosFeedback[int.Parse(user.Id)].Select(i => i.ToString());
            return _userTestFeedback[int.Parse(user.Id)].Select(i => i.ToString());
        }

        protected override IEnumerable<Tuple<string, float>> GetScoredRelevantItems(Model model, Split split, User user)
        {
            return
                GetRelevantItems(split, user)
                    .Select(i => new Tuple<string, float>(i, (float) _scores[int.Parse(user.Id), int.Parse(i)]));
        }

        protected override IEnumerable<Tuple<string, float>> GetScoredCandidateItems(Model model, Split split, User user)
        {
            return
                GetCandidateItems(split, user)
                    .Select(i =>
                    {
                        int uId = int.Parse(user.Id);
                        int iId = int.Parse(i);

                        //if (_scores.ContainsKey(uId, iId))
                            return new Tuple<string, float>(i, (float) _scores[uId, iId]);

                        //return new Tuple<string, float>(i, -1f);
                    });
        }

        protected override string GetEvaluatorName()
        {
            return "UserBasedFile";
        }

    }
}
