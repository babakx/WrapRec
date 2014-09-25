using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Utilities;

namespace CrowdRecDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            RunCrowdRecDemo(args);

            Console.WriteLine("Finished!");
            Console.Read();
        }

        static void RunCrowdRecDemo(string[] args)
        {
            string entitiesFile, relationsFile;

            if (args.Length == 2)
            {
                entitiesFile = args[0];
                relationsFile = args[1];
            }
            else
            {
                entitiesFile = Paths.MovieLensCrowdRecEntities;
                relationsFile = Paths.MovieLensCrowdRecRelations;
            }

            var cr = new CrowdRecDemo(entitiesFile, relationsFile);
            cr.RunDemo();
        }

    }
}
