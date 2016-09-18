using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.IO;

namespace WrapRec.Models
{
    public abstract class Model
    {
        public string Id { get; set; }
        public int PureTrainTime { get; protected set; }
        public int PureEvaluationTime { get; protected set; }
        public Dictionary<string, string> SetupParameters { get; set; }
        public DataType DataType { get; protected set; }
        public abstract void Setup();
        public abstract void Train(Split split);
        public abstract void Evaluate(Split split, EvaluationContext context);
        public abstract float Predict(string userId, string itemId);
        public abstract float Predict(Feedback feedback);
        public abstract void Clear();
        public abstract Dictionary<string, string> GetModelParameters();

        public delegate void ModelIterateEventHandler(object sender, int epochTime);

        public event ModelIterateEventHandler Iterated;

        public void OnIterate(object sender, int epochTime)
        {
            if (Iterated != null)
                Iterated(sender, epochTime);
        }
    }
}
