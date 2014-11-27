using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MyMediaLite.Data;

namespace MusicRec
{
    public class TagManager
    {
        public string TagFile { get; set; }

        public TagManager(string tagFile)
        {
            TagFile = tagFile;
        }

        public void CreateLdaInput(string outputFile)
        {
            var map = new Mapping();

            var outputLines = File.ReadAllLines(TagFile).Select(l =>
            {
                var tokens = l.Split('\t');
                int tagCount = Convert.ToInt32(tokens[1]);
                string output = "";
                if (tagCount > 0)
                {
                    var tags = tokens[2].Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
                    output += map.ToInternalID(tokens[0]);

                    foreach (var tag in tags)
                    {
                        var parts = tag.Split(new string[] { "::" }, StringSplitOptions.None);
                        output += " " + map.ToInternalID(parts[0]) + ":" + parts[1];
                    }
                }

                return output;
            }).Where(o => !String.IsNullOrEmpty(o));


            File.WriteAllLines(outputFile, outputLines);
        }
    }
}
