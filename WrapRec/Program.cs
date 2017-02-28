using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Utils;
using WrapRec.Core;
using System.IO;

namespace WrapRec
{

	class Program
	{
		static void Main(string[] args)
		{
            try
            {
                string usage = @"WrapRec 2.0 recommendation toolkit. \nUsage: WrapRec.exe configFile [cwd=current-working-directory] [params]";

                if (args.Length < 1)
                {
                    Console.WriteLine(usage);
                    return;
                }

                ExperimentManager em;

                if (args.Length >= 2)
                {
                    var argsDic = args.Skip(1).ToDictionary(a => a.Split('=')[0], a => a.Split('=')[1]);

                    if (argsDic.ContainsKey("cwd"))
                    {
                        Environment.CurrentDirectory = argsDic["cwd"];
                        argsDic.Remove("cwd");
                    }
                    em = new ExperimentManager(args[0], argsDic);
                }
                else
                {
                    em = new ExperimentManager(args[0]);
                }

                em.RunExperiments();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

#if DEBUG
            Console.Read();
#endif
		}
        
    }
}

