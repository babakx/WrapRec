using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return new { Name = parts[0], Vale = parts[1] };
            }).ToDictionary(p => p.Name, p => p.Vale);

            var problem = Parameters["problem"].ToLower();
            
            if (problem == "rp")
                RunRatingPrediction();
            else if (problem == "ir")
                RunItemRecommendation();
            else
                Console.WriteLine("Parameter 'problem' should be specified and should be either 'rp' or 'ir'");
        }

        private void RunRatingPrediction()
        { 
            
        }

        private void RunItemRecommendation()
        { }
    }
}
