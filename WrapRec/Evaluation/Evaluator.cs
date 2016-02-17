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
    public abstract class Evaluator
    {
		public Dictionary<string, string> SetupParameters { get; set; }
		public virtual void Setup()
		{ 
			// Empty body
		}
		public abstract void Evaluate(EvaluationContext context, Model model, Split split);
    }
}
