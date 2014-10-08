using CrowdRecDemo;
using CsvHelper.Configuration;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Evaluation;
using WrapRec.Readers;
using WrapRec.Recommenders;
using WrapRec.Utilities;

namespace WrapRecDemo
{
    public class Setup
    {
        public Dictionary<string, string> Parameters { get; private set; }
        public Setup(string[] args)
        {
            Parameters = args.Select(arg =>
            {
                string[] parts = arg.Split('=');
                return new { Name = parts[0].Substring(2), Vale = parts[1] };
            }).ToDictionary(p => p.Name, p => p.Vale);

            CheckParameters();

            var problem = Parameters["problem"].ToLower();

            if (problem == "rp")
            {
                Console.WriteLine("\nProblem: Rating Prediction");
                RunRatingPrediction();
            }
            else if (problem == "ir")
            {
                Console.WriteLine("\nProblem: Item Recommendation");
                RunItemRecommendation();
            }
            else
                Console.WriteLine("Parameter 'problem' should be specified and should be either 'rp' or 'ir'");
        }

        private void CheckParameters()
        {
            var parameters = new string[] { "problem", "dataset", "data-format", "csv-sep", "testportion", "trainfile", "testfile", "entitiesfile",
                "relationsfile", "rp-algorithm", "ir-algorithm", "rp-eval", "ir-eval", "cross-domain", "libfm-path"};

            Parameters.Keys.ToList().ForEach(k => 
            {
                if (!parameters.Contains(k))
                    throw new WrapRecException(string.Format("\nInvalid parameter: '{0}'\n", k));
            });
        }

        private Dataset<T> GetDataset<T>(string path1, string path2, CsvClassMap<T> map)
        {
            IDatasetReader<T> reader1 = null, reader2 = null;
            Dataset<T> dataset;

            switch (Parameters.GetValueOrDefault("data-format", ""))
            {
                case ("csv"):
                    string sep = ",";

                    if (Parameters.GetValueOrDefault("csv-sep", "") != "")
                        sep = Parameters["csv-sep"];

                    var csvConfing = new CsvConfiguration() { Delimiter = sep };

                    reader1 = new CsvReader<T>(path1, csvConfing, map);
                    if (path2 != "")
                        reader2 = new CsvReader<T>(path2, csvConfing, map);
                    break;
                case(""):
                    throw new WrapRecException("The parameter 'data-format' should be specified.");
                    break;
            }

            if (path2 == "")
            {
                if (Parameters.GetValueOrDefault("testportion", "") == "")
                    throw new WrapRecException("The parameter 'testportion' should be specified.");

                dataset = new Dataset<T>(reader1, float.Parse(Parameters["testportion"]));
            }
            else
                dataset = new Dataset<T>(reader1, reader2);

            return dataset;
        }

        private IModel GetRecommender()
        {
            switch (Parameters.GetValueOrDefault("rp-algorithm", ""))
            { 
                case("mf"):
                    return new MediaLiteRatingPredictor(new MatrixFactorization());
                case("bmf"):
                    return new MediaLiteRatingPredictor(new BiasedMatrixFactorization());
                case("fm"):
                    var libfmPath = Parameters.GetValueOrDefault("libfm-path", "") != "" ? Parameters["libfm-path"] : "libfm.exe";
                    return new LibFmTrainTester(libFmPath: libfmPath);
            }

            switch (Parameters.GetValueOrDefault("ir-algorithm", ""))
            { 
                case("bpr"):
                    return new MediaLiteItemRecommender(new BPRMF());
                case("mp"):
                    return new MediaLiteItemRecommender(new MostPopular());
                case(""):
                default:
                    throw new WrapRecException("Either of the two parameters 'rp-algorithm' or 'ir-algorithm' should be specified.");
            }
        }
        
        private void RunRatingPrediction()
        {
            // step 1: dataset
            Dataset<ItemRating> dataset;

            if (Parameters.GetValueOrDefault("data-format", "") == "crowdrec")
            {
                string efile = Parameters.GetValueOrDefault("entitiesfile", "");
                string rfile = Parameters.GetValueOrDefault("relationsfile", "");

                if (efile == "" || rfile == "")
                    throw new WrapRecException("The parameters 'entitesfile' and 'relationsfile' should be specified.");

                var reader = new CrowdRecDataReader(efile, rfile);
                var container = new CrowdRecDataContainer();
                reader.LoadData(container);

                if (Parameters.GetValueOrDefault("testportion", "") == "")
                    throw new WrapRecException("The parameter 'testportion' should be specified.");

                dataset = new ItemRatingDataset(container, float.Parse(Parameters["testportion"]));
            }
            else if (Parameters.GetValueOrDefault("dataset", "") != "")
            {
                dataset = GetDataset<ItemRating>(Parameters["dataset"],"", new ItemRatingMap());
            }
            else if (Parameters.GetValueOrDefault("trainfile", "") != "" && Parameters.GetValueOrDefault("testfile", "") != "")
            {
                dataset = GetDataset<ItemRating>(Parameters["trainfile"], Parameters["testfile"], new ItemRatingMap());
            }
            else
                throw new WrapRecException("Not enough information about dataset. Check help!");

            // step 2: recommender
            IModel recommender = GetRecommender();

            Console.WriteLine("\nRecommender: {0}\n", recommender.GetType().Name);

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            ep.Run();            
        }

        private void RunItemRecommendation()
        {
            // step 1: dataset
            Dataset<ItemRanking> dataset;

            if (Parameters.GetValueOrDefault("dataset", "") != "")
            {
                dataset = GetDataset<ItemRanking>(Parameters["dataset"], "", new ItemRankingMap());
            }
            else if (Parameters.GetValueOrDefault("trainfile", "") != "" && Parameters.GetValueOrDefault("testfile", "") != "")
            {
                dataset = GetDataset<ItemRanking>(Parameters["trainfile"], Parameters["testfile"], new ItemRankingMap());
            }
            else
                throw new WrapRecException("Not enough information about dataset. Check help!");

            // step 2: recommender
            IModel recommender = GetRecommender();

            // step3: evaluation
            var ep = new EvaluationPipeline<ItemRanking>(new ItemRankingEvaluationContext(recommender, dataset));
            ep.Evaluators.Add(new NDCG());
            ep.Evaluators.Add(new PrecisionAndRecall(10));
            ep.Evaluators.Add(new ReciprocalRank());

            ep.Run();
        }
    }
}
