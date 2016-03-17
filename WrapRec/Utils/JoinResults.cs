using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using System.IO;
using WrapRec.Utils;

namespace WrapRec.Utils
{
	public class JoinResults : Experiment
	{
		public string[] ResultFiles { get; set; }

		public string Delimiter { get; set; }

		public string OutputFile { get; set; }

		public override void Setup()
		{
			if (SetupParameters.ContainsKey("delimiter"))
				Delimiter = SetupParameters["delimiter"].Replace("\\t", "\t");
			else
				Delimiter = "\t";

			ResultFiles = SetupParameters["sourceFiles"].Split(',')
				.Select(eId => Path.Combine(ExperimentManager.ResultsFolder, eId)).ToArray();

			OutputFile = Path.Combine(ExperimentManager.ResultsFolder, SetupParameters["outputFile"]);
		}

		public override void Run()
		{
			var resultDics = new List<Dictionary<string, string>>();

			// read all files
			foreach (string file in ResultFiles)
			{
				var lines = File.ReadAllLines(file);
				List<string> header = null;

				foreach (string line in lines)
				{
					var values = line.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();

					if (line.StartsWith("ExpeimentId"))
						header = values;
					else
						resultDics.Add(header.Zip(values, (a, b) => new KeyValuePair<string, string>(a, b))
							.ToDictionary(kv => kv.Key, kv => kv.Value));
				}
			}

			var joinedDics = resultDics.JoinDictionaries();

			Logger.Current.Info("Writing joint output...");

			string outputHeader = joinedDics.First().Select(kv => kv.Key).Aggregate((a, b) => a + Delimiter + b);
			var outputLines = joinedDics.Select(dic => dic.Values.Aggregate((a, b) => a + Delimiter + b)).ToList();
			File.WriteAllLines(OutputFile, new string[] { outputHeader }.Concat(outputLines));
		}
	}
}
