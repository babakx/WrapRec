using RF2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using LinqLib.Operators;
using System.IO;
using CenterSpace.NMath.Core;
using CenterSpace.NMath.Stats;
using CenterSpace.NMath.Matrix;
using System.Data;
using RF2.Utilities;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics;
using MyMediaLite.Data;

namespace RF2
{
    public class Clusterer
    {
        IDataset<ItemRating> _dataset;
        Mapping _userMapping, _itemMapping;

        public Clusterer(IDataset<ItemRating> dataset)
        {
            _dataset = dataset;
            _userMapping = new Mapping();
            _itemMapping = new Mapping();
        }

        public void WriteUsersCluster(string path, int numClusters, int numRatings)
        {
            WriteCluster(path, numClusters, numRatings, ir => ir.User.Id, true, path + ".c");
        }

        public void WriteItemsCluster(string path, int numClusters, int numRatings)
        {
            WriteCluster(path, numClusters, numRatings, ir => ir.Item.Id, true, path + ".c");
        }

        private void WriteCluster(string path, int numClusters, int numRatings, Func<ItemRating, string> fieldSelector,
            bool writeRatingHistogram, string centerPath = "")
        {            
            Console.WriteLine("Prepating data for clustering...");

            var items = _dataset.AllSamples.GroupBy(ir => fieldSelector(ir))
                .Select(g => new
                {
                    ItemId = g.Key,
                    RatingVector = g.GroupBy(ir => ir.Rating)
                        .Select(gg => new { Rate = Convert.ToInt32(gg.Key), Count = gg.Count() }).OrderBy(v => v.Rate)
                }).OrderBy(u => u.ItemId).ToList();


            int numItems = items.Count();

            double[,] features = new double[numItems, numRatings];
            Dictionary<string, string> histograms = new Dictionary<string,string>();

            int ix = 0;
            items.ForEach(i =>
            {
                string histLine = "";

                for (int j = 0; j < numRatings; j++)
                {
                    features[ix, j] = i.RatingVector.Where(rv => rv.Rate == (j + 1)).Select(rv => rv.Count).SingleOrDefault();
                    histLine += features[ix, j] + (j != (numRatings - 1) ? "," : "");    
                }
                ix++;
                histograms.Add(i.ItemId, histLine);
            });
            
            var clusters = Cluster(items.Select(u => u.ItemId), features, numClusters, centerPath);
            var output = clusters.Select(c => string.Format("{0},{1}", c.Key, c.Value));
            File.WriteAllLines(path, output);

            if (writeRatingHistogram)
            {
                Console.WriteLine("Writing item histograms...");

                var hist = histograms.Join(clusters, h => h.Key, c => c.Key, (h, c) => new { Hist = h, Cluster = c })
                    .OrderBy(i => i.Cluster.Value);

                var histOutput = hist.Select(h => string.Format("{0},{1},{2}", 
                    h.Cluster.Value, 
                    h.Hist.Key, 
                    h.Hist.Value
                ));

                var header = new string[] { "ClusterId,ItemId,R1,R2,R3,R4,R5" };

                File.WriteAllLines(path + ".hist", header.Concat(histOutput));
            }
        }

        private Dictionary<string, int> Cluster(IEnumerable<string> itemIds, double[,] features, int numClusters, string centerPath = "",
            ClusteringAlgorithm algorithm = ClusteringAlgorithm.KMeans)
        {
            Console.WriteLine("Clustering...");

            features = Normalized(features);

            Console.WriteLine("Features normalized.");

            var dm = new DoubleMatrix(features);
            ClusterSet clusters = null;

            if (algorithm == ClusteringAlgorithm.KMeans)
            {
                var km = new KMeansClustering(dm);
                km.Cluster(numClusters);
                
                Console.WriteLine("Num Clusters: {0}, Num Items: {1}, Num Iterations: {2}", km.K, km.N, km.Iterations);

                if (centerPath != "")
                {
                    var cWriter = new StreamWriter(centerPath);
                    km.FinalCenters.WriteAsCSV(cWriter);
                    cWriter.Close();
                }

                clusters = km.Clusters;
            }
            else
            {
                var nmf = new NMFClustering<NMFDivergenceUpdate>();

                nmf.Factor(dm, numClusters);

                if (nmf.Converged)
                {
                    var uWriter = new StreamWriter(Paths.AmazonBooksUsersCluster + ".nmf");
                    var iWriter = new StreamWriter(Paths.AmazonBooksItemsCluster + ".nmf");
                    
                    nmf.W.WriteAsCSV(uWriter);
                    nmf.H.WriteAsCSV(iWriter);

                    uWriter.Flush();
                    iWriter.Flush();

                    uWriter.Close();
                    iWriter.Close();

                    File.WriteAllLines(Paths.AmazonBooksUsersCluster + ".con", nmf.Connectivity.ToTabDelimited().Split('\n'));

                    clusters = nmf.ClusterSet;

                    File.WriteAllLines(Paths.AmazonBooksUsersCluster + ".cluster", clusters.Clusters.Select(c => c.ToString()));

                    Console.WriteLine("Successfully wrote decompose matrixes.");

                }
                else
                {
                    Console.WriteLine("Factorization failed to converge in {0} iterations.", nmf.MaxFactorizationIterations);
                }

                
            }

            return itemIds.Zip(clusters.Clusters, (i, c) => new { ItemId = i, Cluster = c }).ToDictionary(i => i.ItemId, i => i.Cluster);
        }

        public void ClusterNmf(int numClusters, string path)
        {
            var data = _dataset.AllSamples.Select(ir => new
            {
                Row = _userMapping.ToInternalID(ir.User.Id),
                Col = _itemMapping.ToInternalID(ir.Item.Id),
                Value = Convert.ToDouble(ir.Rating)
            }).ToList();

            File.WriteAllLines(path + ".data", data.Select(d => string.Format("{0},{1},{2}", d.Row, d.Col, d.Value)).ToList());

            var rows = data.Select(d => d.Row).ToArray();
            var colums = data.Select(d => d.Col).ToArray();
            var values = data.Select(d => d.Value).ToArray();

            int numRows = rows.Max();
            int numCols = colums.Max();

            Console.WriteLine("Creating sparse matrix...\nNum users: {0} Num items: {1} Num ratings: {2}", 
                numRows, numCols, values.Count());

            SparseCompressedColumnMatrix sparseMatrix = Matrix.CreateSparse(numRows + 1, numCols + 1, rows, colums, values);

            var nmf = new NonNegativeMatrixFactorization(sparseMatrix, numClusters);

            Console.WriteLine("Decomposing...");
            nmf.Decompose();

            Console.WriteLine("Writing decompose matrixes...");

            var lf = nmf.LeftFactor.ToArray(MatrixElementOrder.RowMajor);
            var rf = nmf.RightFactor.ToArray(MatrixElementOrder.RowMajor);
            
            var lfWriter = new StreamWriter(path + ".lf");
            var rfWriter = new StreamWriter(path + ".rf");

            

            for (int i = 0; i < lf.Length; i++)
            {
                if (i != 0 && i % numClusters == 0)
                {
                    lfWriter.WriteLine();
                }
                lfWriter.Write(lf[i] + " ");
            }

            lfWriter.Close();

            for (int i = 0; i < rf.Length; i++)
            {
                if (i != 0 && i % numClusters == 0)
                {
                    rfWriter.WriteLine();
                }
                rfWriter.Write(rf[i] + " ");
            }

            rfWriter.Close();

            Console.WriteLine("Finished writing.");
        }

        private double[,] Normalized(double[,] rawData)
        {
            // normalize raw data by computing (x - mean) / stddev
            // primary alternative is min-max:
            // v' = (v - min) / (max - min)

            int numSamples = rawData.GetUpperBound(0);
            int numFeatures = rawData.GetUpperBound(1);

            // make a copy of input data
            double[,] result = new double[numSamples, numFeatures];
            for (int i = 0; i < numSamples; ++i)
                for (int j = 0; j < numFeatures; j++)
                    result[i, j] = rawData[i, j];

            for (int i = 0; i < numSamples; i++)
            {
                double colSum = 0.0;
                for (int j = 0; j < numFeatures; j++)
                {
                    colSum += result[i, j];
                }

                double mean = colSum / numFeatures;
                double sum = 0.0;
                for (int j = 0; j < numFeatures; ++j)
                    sum += (result[i, j] - mean) * (result[i, j] - mean);

                double sd = sum / numFeatures;
                for (int j = 0; j < numFeatures; ++j)
                    result[i,j] = (result[i,j] - mean) / sd;
            }
            
            return result;
        }


    }


    public enum ClusteringAlgorithm
    { 
        KMeans,
        NMF
    }
    
}
