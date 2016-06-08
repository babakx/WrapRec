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
            string usage = @"WrapRec 2.0 recommendation toolkit. \nUsage: WrapRec.exe configFile [--cwd=current-working-directory]";

			if (args.Length < 1 || args.Length > 2)
			{
				Console.WriteLine(usage);
				return;
			}
			else if (args.Length == 2)
			{
				var parts = args[1].Split('=');
				if (parts[0] != "--cwd")
				{
					Console.WriteLine("Invalid arguments!");
					return;
				}
                
				Environment.CurrentDirectory = parts[1];
			}

			var em = new ExperimentManager(args[0]);
			em.RunExperiments();

#if DEBUG
            Console.Read();
#endif
		}
        
    }
}

