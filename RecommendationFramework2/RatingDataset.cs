using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RF2.Data;

namespace RF2
{
    public class RatingDataset : Dataset<Rating>
    {
        public RatingDataset(Data.Dataset dataset)
        {
            _allSamples = dataset.Ratings.AsQueryable();
            _trainSamples = _testSamples = _evalSamples = Enumerable.Empty<Rating>().AsQueryable();
        }

        public RatingDataset(Data.Dataset dataset, RecSysContext context, ISplitter splitter)
        {
            _allSamples = context.Ratings.Where(r => r.DatasetId == dataset.Id);
            splitter.Split(dataset, context, ref _trainSamples, ref _testSamples, ref _evalSamples);
        }
    }
}
