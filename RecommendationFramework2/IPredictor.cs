using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public interface IPredictor<T> : IModel
    {
        void Train(IEnumerable<T> trainSet);
        void Predict(T sample);
        bool IsTrained { get; }
    }
}
