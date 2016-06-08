using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WrapRec.Utils
{
    public class PredictionsIntersection : JoinResults
    {

        public override void Run()
        {
            var cutoffs = new int[] { 5, 10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            var preds1 = GetUsersPred(ResultFiles[0]);
            var preds2 = GetUsersPred(ResultFiles[1]);

            int commonUsers = 0;
            var jaccardAtCutoff = new Dictionary<int, float>();

            foreach (int c in cutoffs)
            {
                jaccardAtCutoff.Add(c, 0);
            }

            foreach (var kv in preds1)
            {
                if (preds2.ContainsKey(kv.Key))
                {
                    commonUsers++;

                    foreach (int c in cutoffs)
                    {
                        int intersect = kv.Value.Take(c).Intersect(preds2[kv.Key].Take(c)).Count();
                        jaccardAtCutoff[c] += intersect * 1.0f / (2 * c - intersect);
                    }
                }
            }

            foreach (int c in cutoffs)
            {
                jaccardAtCutoff[c] /= commonUsers;
            }

            var preds1Items = preds1.Values.SelectMany(v => v).Distinct();
            var preds2Items = preds2.Values.SelectMany(v => v).Distinct();

            int commonItems = preds1Items.Intersect(preds2Items).Count();

            var header = new string[] { "CutOff,Similarity,Users1,Users2,CommonUsers,Items1,Items2,CommonItems" };
            string stat = string.Format("{0},{1},{2},{3},{4},{5}",
                preds1.Count, preds2.Count, commonUsers, preds1Items.Count(), preds2Items.Count(), commonItems);

            var values = jaccardAtCutoff.Select(kv => string.Format("{0},{1},{2}", kv.Key, kv.Value, stat));

            File.WriteAllLines(OutputFile, header.Concat(values));
        }

        private Dictionary<string, List<string>> GetUsersPred(string fileName)
        {
            return File.ReadAllLines(fileName).Select(l =>
            {
                var parts = l.Split(' ');
                string userId = parts[0];
                var itemIds = parts.Skip(1).Select(i => i.Split(':')[1]).ToList();
                return new { UserId = userId, ItemIds = itemIds };
            })
            .ToDictionary(i => i.UserId, i => i.ItemIds);
        }
    }
}
