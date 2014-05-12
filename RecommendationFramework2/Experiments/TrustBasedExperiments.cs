using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RF2.Data;
using CsvHelper.Configuration;
using RF2.Readers;
using System.IO;
using RF2.Recommenders;
using MyMediaLite.RatingPrediction;
using MyMediaLite.DataType;
using RF2.Evaluation;
using RF2.Data.Splitters;
using RF2.Utilities;
using MyMediaLite.ItemRecommendation;
using RF2.Entities;

namespace RF2.Experiments
{
    public class TrustBasedExperiments
    {

        public TrustBasedExperiments()
        {

        }
        
        public void Run(int testNum = 2)
        {
            switch (testNum)
            { 
                case(1):
                    TrainAndTest();
                    break;
                case(2):
                    TestExplicitTrust();
                    break;
                case(3):
                    SplitDataset();
                    break;
                case(4):
                    TestImplicitTrust();
                    break;
                case (5):
                    TestTrustWithFM();
                    break;
                default:
                    break;
            }
        }

        public void TrainAndTest()
        {
            // step 1: dataset            
            var dataContext = DataManager.GetDataContext();
            var modelDataset = dataContext.Datasets.Where(d => d.Name.ToLower() == "epinion").Single();
            var dataset = new RatingDataset(modelDataset, dataContext, new TrainTestSplitter(0.3));
            
            // step 2: recommender
            var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());

            // step3: evaluation
            var ep = new EvaluationPipeline<Rating>(new EvalutationContext<Rating>(recommender, dataset));
            //ep.Evaluators.Add(new RMSE());

            ep.Run();
        }

        public void SplitDataset()
        {
            FileHelper.SplitLines(Paths.EpinionRatings, Paths.EpinionTrain50, Paths.EpinionTest50, 0.5, true, true);
        }

        public void TestExplicitTrust()
        {
            //var socialReguls = new float[] { 0.1F, 0.2F, 0.5F, 0.8F, 1F, 1.5F, 2F, 3F, 5F};
            //var numFactors = new uint[] {2, 5, 10, 15, 20};

            var socialReguls = new float[] { 1 };
            var numFactors = new uint[] { 5, 10 };
            

            // step 1: dataset            
            var config = new CsvConfiguration();
            config.Delimiter = " ";

            var trainReader = new CsvReader<ItemRating>(Paths.EpinionTrain75, config, new ItemRatingMap());
            var testReader = new CsvReader<ItemRating>(Paths.EpinionTest25, config, new ItemRatingMap());
            var dataset = new Dataset<ItemRating>(trainReader, testReader);

            var relations = File.ReadAllLines(Paths.EpinionRelations).ToCsvDictionary(' ')
                .Select(i => new Relation() { UserId = i["UserId"], ConnectedId = i["ConnectionId"], DatasetId = 1 });

            foreach (uint num in numFactors)
            {
                string rmseValues = "", maeValues = "";
                foreach (float regul in socialReguls)
                {
                    // step 2: recommender
                    var algorithm = new MatrixFactorization();
                    algorithm.NumFactors = num;
                    //algorithm.SocialRegularization = regul;

                    var recommender = new MediaLiteRatingPredictor(algorithm, relations);

                    // step3: evaluation
                    var context = new EvalutationContext<ItemRating>(recommender, dataset);
                    var ep = new EvaluationPipeline<ItemRating>(context);
                    ep.Evaluators.Add(new RMSE());
                    ep.Evaluators.Add(new MAE());

                    ep.Run();

                    rmseValues += context["RMSE"] + "\t";
                    maeValues += context["MAE"] + "\t";
                }
                Console.WriteLine(num + "\t" + rmseValues + "\t" + maeValues);
            }

        }

        public void TestImplicitTrust()
        {
            var socialReguls = new float[] { 1F };
            var numFactors = new uint[] {5, 10};
            var trustScores = new string[] { "trust_values_LATHIA.dat", "trust_values_HWANGCHEN.dat",
                "trust_values_ODONOVAN.dat", "trust_values_PEARSON.dat", "trust_values_SHAMBOURLU.dat" };

            // step 1: dataset            
            var config = new CsvConfiguration();
            config.Delimiter = " ";

            var trainReader = new CsvReader<ItemRating>(Paths.EpinionTrain80, config, new ItemRatingMap());
            var testReader = new CsvReader<ItemRating>(Paths.EpinionTest20, config, new ItemRatingMap());
            var dataset = new Dataset<ItemRating>(trainReader, testReader);

            foreach (string scoreFile in trustScores)
            {
                var relations = File.ReadAllLines(Paths.EpinionRelationsImplicit + scoreFile).ToCsvDictionary('\t')
                    .Select(i => new Relation()
                    {
                        UserId = i["UserId"],
                        ConnectedId = i["ConnectionId"],
                        ConnectionStrength = float.Parse(i["Strength"])
                    });
                //.Where(r => r.ConnectionStrength > 1F);

                string rmseValues = "", maeValues = "";

                foreach (uint num in numFactors)
                {
                    // step 2: recommender
                    var algorithm = new SocialMF();
                    algorithm.SocialRegularization = 1;
                    algorithm.NumFactors = num;

                    var recommender = new MediaLiteRatingPredictor(algorithm, relations);

                    // step3: evaluation
                    var context = new EvalutationContext<ItemRating>(recommender, dataset);
                    var ep = new EvaluationPipeline<ItemRating>(context);
                    ep.Evaluators.Add(new RMSE());
                    ep.Evaluators.Add(new MAE());

                    ep.Run();
                    rmseValues += context["RMSE"] + "\t";
                    maeValues += context["MAE"] + "\t";
                }

                Console.WriteLine(scoreFile + "\t" + rmseValues + "\t" + maeValues);

            }

        }

        public void TestTrustWithFM()
        {
            // step 1: dataset            
            var config = new CsvConfiguration();
            config.Delimiter = " ";

            var trainReader = new CsvReader<ItemRating>(Paths.EpinionTrain80, config, new ItemRatingMap());
            var testReader = new CsvReader<ItemRating>(Paths.EpinionTest20, config, new ItemRatingMap());

            var relations = File.ReadAllLines(Paths.EpinionRelationsImplicit).ToCsvDictionary('\t')
                .Select(i => new Relation()
                {
                    UserId = i["UserId"],
                    ConnectedId = i["ConnectionId"],
                    ConnectionStrength = float.Parse(i["Strength"])
                }).ToList();
                //.Where(r => r.ConnectionStrength > 1F);


            var trainWithRelations = new ItemRatingWithRelationReader(trainReader, relations);
            var testWithRelations = new ItemRatingWithRelationReader(testReader, relations);

            var dataset = new Dataset<ItemRatingWithRelations>(trainWithRelations, testWithRelations);

            Console.WriteLine("Features constructed.");

            // step 2: recommender
            var recommender = new LibFmTrainTester();

            // step3: evaluation
            var context = new EvalutationContext<ItemRating>(recommender, dataset);
            var ep = new EvaluationPipeline<ItemRating>(context);
            ep.Evaluators.Add(new RMSE());

            ep.Run();
        }


    }
}
