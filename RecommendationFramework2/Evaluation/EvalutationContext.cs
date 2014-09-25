using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Evaluation
{
    public class EvalutationContext<T>
    {
        Dictionary<string, object> _items;
        
        public IModel Model { get; set; }
        public IDataset<T> Dataset { get; set; }
        public bool IsTested { get; set; }
        
        public EvalutationContext(IModel model, IDataset<T> dataset)
        {
            Model = model;
            Dataset = dataset;
            _items = new Dictionary<string, object>();
        }

        public void RunDefaultTrainAndTest()
        {
            // check if the test is alreay accomplished
            if (IsTested)
                return;

            if (Model is ITrainTester<T>)
            {
                ((ITrainTester<T>)Model).TrainAndTest(Dataset.TrainSamples, Dataset.TestSamples);
            } 
            else if (Model is IPredictor<T>)
            {
                var predictor = (IPredictor<T>)Model;

                // check if the recommender is trained
                if (!predictor.IsTrained)
                    predictor.Train(Dataset.TrainSamples);

                Console.WriteLine("Testing on test set...");

                foreach (var sample in Dataset.TestSamples)
                {
                    predictor.Predict(sample);
                }
            }
            else
                throw new Exception("The model is not supported for test on test set.");
            
            IsTested = true;
        }

        public object this[string name]
        {
            get
            {
                return _items[name];
            }
            set
            {
                _items[name] = value;
            }
        }

        public Dictionary<string, object> Items
        {
            get { return _items; }
        }
    }
}
