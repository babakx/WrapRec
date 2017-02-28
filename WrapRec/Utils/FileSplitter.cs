using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using LinqLib.Sequence;

namespace WrapRec.Utils
{
	public class FileSplitter : Experiment
	{
		public string Part1Path { get; set; }
		public string Part2Path { get; set; }
		public string SourcePath { get; set; }
		public float Part1Ratio { get; set; }
		public bool Shuffle { get; set; }
		public bool HasHeader { get; set; }

		public override void Setup()
		{
			var readerPaths = ExperimentManager.ConfigRoot.Descendants("reader")
				.ToDictionary(el => el.Attribute("id").Value.Inject(ExperimentManager.Parameters), el => el.Attribute("path").Value.Inject(ExperimentManager.Parameters));

			Part1Path = readerPaths[SetupParameters["part1Reader"]];
			Part2Path = readerPaths[SetupParameters["part2Reader"]];
			SourcePath = readerPaths[SetupParameters["sourceReader"]];
			
			Part1Ratio = float.Parse(SetupParameters["part1Ratio"]);

			var hasHeaderAttr = ExperimentManager.ConfigRoot.Descendants("reader")
				.Where(el => el.Attribute("id").Value.Inject(ExperimentManager.Parameters) == SetupParameters["sourceReader"]).Single()
				.Attribute("hasHeader");

			HasHeader = hasHeaderAttr != null && hasHeaderAttr.Value == "false" ? false : true;

			Shuffle = SetupParameters.ContainsKey("shuffle") && SetupParameters["shuffle"] == "false" ? false : true;
		}

		public override void Run()
		{
			Logger.Current.Info("Splitting lines...");
			SplitLines(SourcePath, Part1Path, Part2Path, Part1Ratio, HasHeader, Shuffle);
		}

		public static void SplitLines(string source, string part1Path, string part2Path, double part1Ratio, bool hasHeader, bool shuffle)
		{
			var lines = File.ReadLines(source);

			IEnumerable<string> header = Enumerable.Empty<string>();

			if (hasHeader)
			{
				header = lines.Take(1);
				lines = lines.Skip(1);
			}

			if (shuffle)
				lines = lines.Shuffle();

			int part1Count = Convert.ToInt32(lines.Count() * part1Ratio);

			var part1 = lines.Take(part1Count).ToList();
			var part2 = lines.Skip(part1Count).ToList();

			File.WriteAllLines(part1Path, header.Concat(part1));
			File.WriteAllLines(part2Path, header.Concat(part2));
		}
	}
}
