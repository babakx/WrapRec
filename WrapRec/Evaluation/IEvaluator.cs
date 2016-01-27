using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Models;

namespace WrapRec.Evaluation
{
    public interface IEvaluator
    {
        void Evaluate(EvaluationContext context, Model model, Split split);
    }
}
