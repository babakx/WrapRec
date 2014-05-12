using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RF2
{
    public class XDomainClusterer
    {
        string _targetPath, _auxPath, _auxCode;

        public XDomainClusterer(string targetPath, string auxPath, string auxCode)
        {
            _targetPath = targetPath;
            _auxPath = auxPath;
            _auxCode = auxCode;
        }

        public void WriteClusterMemberships()
        {
            string histPath = _targetPath + ".csv.hist";

            Console.WriteLine("Calculating cluster membership...");

            var histograms = File.ReadAllLines(histPath).ToCsvDictionary()
                .Select(i => new RatingHistogram5()
                {
                    ItemId = i["ItemId"],
                    R1 = Convert.ToInt32(i["R1"]),
                    R2 = Convert.ToInt32(i["R2"]),
                    R3 = Convert.ToInt32(i["R3"]),
                    R4 = Convert.ToInt32(i["R4"]),
                    R5 = Convert.ToInt32(i["R5"])
                }).ToList();

            var cMemberships = GetClusterMembership(histograms);

            var output = cMemberships.Select(hc => string.Format("{0},{1}", hc.Key, hc.Value));

            Console.WriteLine("Writing memberships...");

            File.WriteAllLines(_targetPath + ".csv." + _auxCode, output);
        }

        private Dictionary<string, int> GetClusterMembership(IList<RatingHistogram5> histograms)
        {
            string auxCCPath = _auxPath + ".csv.c";

            var clusterCenters = File.ReadAllLines(auxCCPath)
                .Select(l =>
                {
                    double[] elements = l.Split(',').Select(e => Convert.ToDouble(e)).ToArray();
                    return elements;
                }).ToList();

            return histograms.ToDictionary(h => h.ItemId, h => GetClusterId(h, clusterCenters));
        }

        private int GetClusterId(RatingHistogram5 hist, IList<double[]> clusterCenters)
        {
            var tuple = hist.ToDoubleArray();

            double closestDistance = double.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < clusterCenters.Count; i++)
            {
                double dist = Distance(tuple, clusterCenters[i]);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private double Distance(double[] tuple, double[] center)
        {
            // Euclidean distance between two vectors for UpdateClustering()
            // consider alternatives such as Manhattan distance
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - center[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }




    }
}
