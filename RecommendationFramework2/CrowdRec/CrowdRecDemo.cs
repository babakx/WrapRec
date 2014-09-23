using MyMediaLite.RatingPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Recommenders;
using WrapRec.Evaluation;

namespace WrapRec.CrowdRec
{
    public class CrowdRecDemo
    {
        string _entitesFile, _relationsFile;

        public CrowdRecDemo(string entitesFile, string relationsFile)
        {
            _entitesFile = entitesFile;
            _relationsFile = relationsFile;
        }

        public void RunDemo()
        { 
            // step 1: load dataset
            var container = new CrowdRecDataContainer();
            var reader = new CrowdRecDataReader(_entitesFile, _relationsFile);
            reader.LoadData(container);

            var dataset = new ItemRatingDataset(container, 0.3f);

            // step 2: recommender
            var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());
            
            // step 3: evaluations
            var pipline = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            pipline.Evaluators.Add(new RMSE());
            pipline.Evaluators.Add(new MAE());

            pipline.Run();
        }
    }
}
