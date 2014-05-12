using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using EntityFramework.Extensions;

namespace RF2.Data.Splitters
{
    public class TrainTestSplitter : ISplitter
    {
        double _testPortion;

        public TrainTestSplitter(double testPortion)
        {
            _testPortion = testPortion;
        }

        public void Split(Dataset dataset, RecSysContext context, ref IQueryable<Rating> trainset, ref IQueryable<Rating> testset, ref IQueryable<Rating> evalset)
        {
            int trainCount = Convert.ToInt32(context.Ratings.Where(r => r.DatasetId == dataset.Id).Count() * (1 - _testPortion));

            trainset = context.Ratings.Where(r => r.DatasetId == dataset.Id).Take(trainCount);
            testset = context.Ratings.Where(r => r.DatasetId == dataset.Id).Skip(trainCount);
            evalset = Enumerable.Empty<Rating>().AsQueryable();
        }

        public override string ToString()
        {
            return string.Format("TrainTest Splitter. Test portion: {0}", _testPortion);
        }

    }
}
