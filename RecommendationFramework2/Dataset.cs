using RF2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class Dataset<T> : IDataset<T>
    {
        protected IQueryable<T> _allSamples;
        protected IQueryable<T> _trainSamples;
        protected IQueryable<T> _testSamples;
        protected IQueryable<T> _evalSamples;

        public Dataset()
        {
            _allSamples = _trainSamples = _testSamples = _evalSamples = Enumerable.Empty<T>().AsQueryable();
        }

        public Dataset(IDatasetReader<T> reader) 
            : this(reader, 0, 0)
        { }

        public Dataset(IDatasetReader<T> reader, double testPortion) 
            : this(reader, testPortion, 0)
        { }

        public Dataset(IDatasetReader<T> reader, double testPortion, double evalPortion)
        {
            if (testPortion + evalPortion > 1)
                throw new Exception("Sum of test and evaluation portions should be less than 1.");

            _allSamples = reader.ReadSamples().AsQueryable();

            if (testPortion == 0 && evalPortion == 0)
            {
                _trainSamples = _allSamples;
                _testSamples = _evalSamples = Enumerable.Empty<T>().AsQueryable();
                return;
            }

            int trainCount = Convert.ToInt32(_allSamples.Count() * (1 - testPortion - evalPortion));
            int testCount = Convert.ToInt32(_allSamples.Count() * testPortion);
            
            _trainSamples = _allSamples.Take(trainCount);
            _testSamples = _allSamples.Skip(trainCount).Take(testCount);
            _evalSamples = _allSamples.Skip(trainCount + testCount);
        }

        public Dataset(IDatasetReader<T> reader, Func<IEnumerable<T>, Tuple<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>>> trainTestEvalSplitter) 
        {
            _allSamples = reader.ReadSamples().AsQueryable();
            
            var splits = trainTestEvalSplitter(_allSamples);

            _trainSamples = splits.Item1.AsQueryable();
            _testSamples = splits.Item2.AsQueryable();
            _evalSamples = splits.Item3.AsQueryable();
        }

        public Dataset(IDatasetReader<T> trainReader, IDatasetReader<T> testReader)
        {
            _trainSamples = trainReader.ReadSamples().AsQueryable();
            _testSamples = testReader.ReadSamples().AsQueryable();
            _evalSamples = Enumerable.Empty<T>().AsQueryable();
            _allSamples = _trainSamples.Concat(_testSamples);
        }


        public IQueryable<T> AllSamples
        {
            get 
            {
                return _allSamples;
            }
        }

        public IQueryable<T> TrainSamples
        {
            get 
            {
                return _trainSamples;
            }
        }

        public IQueryable<T> TestSamples
        {
            get 
            {
                return _testSamples;
            }
        }


        public IQueryable<T> EvalSamples
        {
            get 
            {
                return _evalSamples;
            }
        }
    }
}
