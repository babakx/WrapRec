using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Evaluation
{
    public interface IEvaluator<T>
    {
        void Evaluate(EvalutationContext<T> context);
    }
}
