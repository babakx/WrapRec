using WrapRec.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
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

            _allSamples = reader.ReadAll().AsQueryable();

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
            _allSamples = reader.ReadAll().AsQueryable();
            
            var splits = trainTestEvalSplitter(_allSamples);

            _trainSamples = splits.Item1.AsQueryable();
            _testSamples = splits.Item2.AsQueryable();
            _evalSamples = splits.Item3.AsQueryable();
        }

        public Dataset(IDatasetReader<T> trainReader, IDatasetReader<T> testReader)
        {
            _trainSamples = trainReader.ReadAll().AsQueryable();
            _testSamples = testReader.ReadAll().AsQueryable();
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

    public class ItemRatingDataset : Dataset<ItemRating>
    {
        public ItemRatingDataset(DataContainer trainContainer, DataContainer testContainer)
        {
            _trainSamples = trainContainer.Ratings.AsQueryable();
            _testSamples = testContainer.Ratings.AsQueryable();
        }

        public ItemRatingDataset(DataContainer container)
        {
            //_trainSamples = container.Ratings.Where(r => r.IsTest == false && r.Domain.IsTarget == true).AsQueryable();
            _trainSamples = container.Ratings.Where(r => r.IsTest == false).AsQueryable();
            _testSamples = container.Ratings.Where(r => r.IsTest == true).AsQueryable();
        }
    }
}
