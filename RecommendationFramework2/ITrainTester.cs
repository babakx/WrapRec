using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public interface ITrainTester<T> : IModel
    {
        void TrainAndTest(IEnumerable<T> trainSet, IEnumerable<T> testSet);
    }
}
