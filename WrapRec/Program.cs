using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Utils;
using WrapRec.Core;

namespace WrapRec
{
	class Program
	{
		static void Main(string[] args)
		{
			string usage = @"WrapRec 2.0 recommendation toolkit. \nUsage: WrapRec.exe [configFile]";
			if (args.Length != 1)
			{
				Console.WriteLine(usage);
				return;
			}

			var em = new ExperimentManager(args[0]);
			em.RunExperiments();
			Console.Read();
		}
	}
}
