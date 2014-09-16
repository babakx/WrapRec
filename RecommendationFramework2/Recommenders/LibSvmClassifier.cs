using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSvm;
using System.IO;

namespace WrapRec.Recommenders
{
    public class LibSvmClassifier : ITrainTester<ItemRating>
    {
        public SvmParameter Parameters { get; private set; }

        public LibSvmFeatureBuilder FeatureBuilder { get; set; }

        public Func<ItemRating, double> LabelSelector { get; set; }

        public LibSvmClassifier(SvmParameter parameters, LibSvmFeatureBuilder featureBuilder, Func<ItemRating, double> labelSelector)
        {
            Parameters = parameters;
            FeatureBuilder = featureBuilder;
            LabelSelector = labelSelector;
        }

        public void TrainAndTest(IEnumerable<ItemRating> trainSet, IEnumerable<ItemRating> testSet)
        {
            var problem = new SvmProblem()
            {
                X = trainSet.Select(ir => FeatureBuilder.GetSvmNode(ir)).ToArray(),
                Y = trainSet.Select(ir => LabelSelector(ir)).ToArray()
            };


            Parameters.Check(problem);

            Console.WriteLine("Writing training samples...");
            WriteSvmFile(problem, "train.libsvm");
            
            Console.WriteLine("LibSvm training...");
            
            LibSvm.SvmModel model = Svm.Train(problem, Parameters);

            var predictedClasses = new List<float>();

            Console.WriteLine("LibSvm testing...");
            foreach (var ir in testSet)
            {
                ir.PredictedRating = (float)model.Predict(FeatureBuilder.GetSvmNode(ir));
                predictedClasses.Add(ir.PredictedRating);                    
            }

            Console.WriteLine("Writing output...");
            File.WriteAllLines("output.libsvm", predictedClasses.Select(i => i.ToString()));
        }

        private void WriteSvmFile(SvmProblem problem, string fileName)
        {
            var samples = problem.Y.Zip(problem.X, (y, x) => string.Format("{0} {1}",
                y,
                x.Select(n => string.Format("{0}:{1}", n.Index, n.Value)).Aggregate((a, b) => a + " " + b)));

            File.WriteAllLines(fileName, samples);
        }
    }
}
