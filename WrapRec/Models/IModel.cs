﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;

namespace WrapRec.Models
{
    public interface IModel
    {
		void Setup(Dictionary<string,string> modelParams);
		void Train(ISplit split);
		void Evaluate(ISplit split, EvaluationContext context);
    }
}