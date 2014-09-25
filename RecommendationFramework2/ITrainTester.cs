using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public interface ITrainTester<T> : IModel
    {
        void TrainAndTest(IEnumerable<T> trainSet, IEnumerable<T> testSet);
    }
}
