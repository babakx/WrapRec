using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Readers;
using WrapRec.Evaluation;
using WrapRec.Entities;
using WrapRec.Recommenders;
using WrapRec.Data;
using MyMediaLite.RatingPrediction;
using WrapRec.Utilities;
using System.IO;
using LinqLib.Sequence;

namespace WrapRec.Experiments
{
    public class MovieLensTester
    {
        public MovieLensTester()
        {

        }
        
        public void Run(int testNum = 1)
        {
            switch (testNum)
            { 
                case(1):
                    TestMovieLens();
                    break;
            }
        }

        public void TestMovieLens()
        {
            //var dataset1 = new Dataset<MovieLensItemRating>(new MovieLensReader(Paths.MovieLens1M), 0.3);

            //var c = new Clusterer(dataset1);

            //for (int i = 2; i < 15; i += 2)
            //{
            //    c.WriteCluster(Paths.MovieLens1MItemsCluster + i + ".csv", i);
            //}

            //return;
            
            
            // step 1: dataset            
            var lines = File.ReadAllLines(Paths.MovieLens1M).Shuffle(1).Take(50000).ToList();


            for (int i = 2; i < 15; i += 2)
            {

                //var dataset = new Dataset<MovieLensItemRating>(new MovieLensReader(Paths.MovieLens1M, Paths.MovieLens1MUsersCluster + i + ".csv", Paths.MovieLens1MItemsCluster + i + ".csv", lines), 0.3);

                //var c = new Clusterer(dataset);

                //for (int i = 2; i < 15; i += 2)
                //{
                //    c.WriteCluster(Paths.MovieLens1MUsersCluster + i + ".csv", i);
                //}

                //return;

                // step 2: recommender
                //var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());
                var recommender = new LibFmTrainTester();

                // step3: evaluation
                //var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
                //ep.Evaluators.Add(new RMSE());

                //ep.Run();
            }
        }

    }
}
