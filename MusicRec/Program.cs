using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.Recommenders;
using MyMediaLite.ItemRecommendation;

namespace MusicRec
{
    class Program
    {
        static string path = @"D:\Data\Datasets\Last.fm\babak_user sessions.csv";
        static string tagsPath = @"D:\Data\Datasets\Last.fm\tags_per_babak_2.csv";
        static string ldaInput = @"D:\Data\Datasets\Last.fm\tags_lda_input.txt";
        
        static void Main(string[] args)
        {
            //ProcessTags();
            TrainTest();
            Console.WriteLine("Finished!");
            Console.Read();
        }

        static void TrainTest()
        {
            var reader = new PlayingSessionReader(path);
            var container = new MusicDataContainer();

            reader.LoadData(container);

            var splitter = new PositiveFeedbackSimpleSplitter(container, 0.3f);
            //var itemRecommender = new MostPopular();
            //var model = new MediaLitePosFeedbakItemRecommender(itemRecommender);
            var fm = new PosFeedbackLibFmTrainTester();

            var context = new EvalutationContext<PositiveFeedback>(fm, splitter);

            //var pipline = new EvaluationPipeline<PositiveFeedback>(context);
            //pipline.Evaluators.Add(new MediaLitePositiveFeedbackEvaluators(itemRecommender));

            //pipline.Run();

            context.RunDefaultTrainAndTest();
        }

        static void ProcessTags()
        {
            var tagManager = new TagManager(tagsPath);
            tagManager.CreateLdaInput(ldaInput);
        }
    }
}
