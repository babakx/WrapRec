using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RF2.Evaluation
{
    public class FullUserItemMatrixPredictor : IEvaluator<ItemRating>
    {
        string _outputPath;

        public FullUserItemMatrixPredictor(string outputPath)
        {
            _outputPath = outputPath;
        }
        
        public void Evaluate(EvalutationContext<ItemRating> context)
        {
            if (!(context.Model is IPredictor<ItemRating>))
                throw new Exception("To predict the full user item matrix the model should implement IPredictor<ItemRating>.");

            // make sure the dataset is trained
            context.RunDefaultTrainAndTest();

            Console.WriteLine("Predicting full user-item matrix...");

            var recommender = (IPredictor<ItemRating>)context.Model;
            
            var dataset = context.Dataset;

            var allItemIds = dataset.AllSamples.Select(ir => ir.Item.Id).ToList();
            var allUserIds = dataset.AllSamples.Select(ir => ir.User.Id).ToList();

            var writer = new StreamWriter(_outputPath);

            // header of the file should be the list of all items
            var header = allItemIds.Aggregate("\t", (cur, next) => cur + "\t" + next);

            writer.WriteLine(header);

            allItemIds.ForEach(u => 
            {
                string line = u;
                allUserIds.ForEach(i => 
                {
                    var itemRating = new ItemRating(u, i);
                    recommender.Predict(itemRating);
                    line += string.Format("\t{0:0.00}", itemRating.PredictedRating);
                });
                writer.WriteLine(line);
                writer.Flush();
            });

            writer.Close();
        }
    }
}
