using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Models;

namespace WrapRec.Evaluation
{
    public class Scorer : RankingEvaluators
    {
        public string ScoresFile { get; set; }
        public string InputFile { get; set; }

        public override void Setup()
        {
            ScoresFile = SetupParameters["scoresFile"];
            InputFile = SetupParameters["inputFile"];
            base.Setup();
        }

        public override void Evaluate(EvaluationContext context, Model model, Split split)
        {
            var reader = new StreamReader(InputFile);
            var writer = new StreamWriter(ScoresFile);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',');
                writer.WriteLine(model.Predict(parts[0], parts[1]));
            }

            reader.Close();
            writer.Close();
        }
    }
}
