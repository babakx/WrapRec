using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
	public static class Logger
	{
		// Log levels
		// Trace, Debug, Info, Warn, Error, Fatal
		public static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
	}
}
