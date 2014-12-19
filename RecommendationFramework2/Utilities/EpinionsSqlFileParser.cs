using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WrapRec.Utilities
{
    public class EpinionsSqlFileParser
    {
        string _path;

        public EpinionsSqlFileParser(string path)
        {
            _path = path;
        }

        public void ParseTable(string tableName, string header, string outputFile)
        {
            var records = new List<string>() { header };

            foreach (string line in File.ReadAllLines(_path))
            {
                if (line.StartsWith(string.Format("INSERT INTO `{0}`", tableName)))
                {
                    records.AddRange(line.Substring(line.IndexOf('(') + 1).Split(new string[] { "),(" }, StringSplitOptions.RemoveEmptyEntries));
                    string last = records[records.Count - 1];
                    records[records.Count - 1] = last.Substring(0, last.Length - 2);   // remove ');' from the end of string
                }
            }

            File.WriteAllLines(outputFile, records);
        }
        
        public void ParseCategories(string outputFile)
        {
            ParseTable("Category", "CategoryId,Name,ParentId", outputFile);
        }

        public void ParseProducts(string outputFile)
        {
            ParseTable("Product", "ProductId,CategoryId", outputFile);
        }

        public void ParseExpertise(string outputFile)
        {
            ParseTable("Expertise", "UserId,CategoryId,Experty", outputFile);
        }

        public void ParseReviews(string outputFile)
        {
            ParseTable("Review", "ReviewId,UserId,Rating,ReviewRating,ProductId,Date", outputFile);
        }

        public void ParseSimilarity(string outputFile)
        {
            ParseTable("Similarity", "UserId,SimilarId,Similarity", outputFile);
        }

        public void ParseTrust(string outputFile)
        {
            ParseTable("Trust", "UserId,Trustee,Trust", outputFile);
        }

    }
}
