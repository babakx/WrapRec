using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public interface IDataset<out T>
    {
        IQueryable<T> AllSamples { get; }
        IQueryable<T> TrainSamples { get; }
        IQueryable<T> TestSamples { get; }
        IQueryable<T> EvalSamples { get; }
    }
}
