using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Experiments;
using System.Runtime.InteropServices;
using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using System.IO;
using CenterSpace.NMath.Core;
using CenterSpace.NMath.Stats;

namespace WrapRec
{
    class Program
    {
        static void Main(string[] args)
        {
            //(new MovieLensTester()).Run();
            //(new AmazonTester()).Run();
            //(new Ectel2014Experiments()).Run();
            //(new TrustBasedExperiments()).Run();
            //(new Recsys2014Experiments()).Run();
            (new Journal2014Experiments()).Run();

            Console.WriteLine("Finished!.");

            return;

        }
    }
}
