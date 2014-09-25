using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LinqLib.Sequence;

namespace WrapRec.Utilities
{
    public class FileHelper
    {
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
