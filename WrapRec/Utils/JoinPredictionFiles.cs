using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Utils
{
	public class JoinPredictionFiles : JoinResults
	{
		public override void Run()
		{
			var allLines = ResultFiles.SelectMany(file => File.ReadLines(file).ToCsvDictionary('\t')
				.Select(i => new { UserId = i["UserId"], ItemId = i["ItemId"], Score = i["Score"], IsCorrect = i["IsCorrect"], File = file.GetFileName() }));
			
			var output = allLines.GroupBy(i => new {i.UserId, i.ItemId}).Select(g => 
				string.Format("{0}\t{1}\t{2}", g.Key.UserId, g.Key.ItemId,
					g.Select(a => string.Format("{0}\t{1}\t{2}", a.Score, a.IsCorrect, a.File))
					.Aggregate((a,b) => a + "\t" + b)));

			File.WriteAllLines(OutputFile, output);
		}
	}
}
