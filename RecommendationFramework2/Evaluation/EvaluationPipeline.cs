using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Evaluation
{
    public class EvaluationPipeline<T>
    {
        public EvalutationContext<T> Context { get; private set; }

        public List<IEvaluator<T>> Evaluators { get; private set; }
        
        public EvaluationPipeline(EvalutationContext<T> context)
        {
            Context = context;
            Evaluators = new List<IEvaluator<T>>();
        }

        public void Run()
        {
            Evaluators.ForEach(ev =>
                { 
                    ev.Evaluate(Context); 
                });
        }
    }
}
