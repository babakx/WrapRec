using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Models;
using WrapRec.Evaluation;
using WrapRec.Data;

namespace WrapRec.Core
{
    public class Experiment : IExperiment
    {
        public IList<IModel> Models { get; private set; }
        public IList<ISplit> Splits { get; private set; }
        public EvaluationContext EvaluationContext { get; set; }
        public string Id { get; set; }
        public int Repeat { get; set; }

        public Experiment()
        {
            Models = new List<IModel>();
            Splits = new List<ISplit>();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
