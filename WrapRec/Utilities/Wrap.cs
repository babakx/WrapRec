using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Utilities
{
	public static class Wrap
	{
		/// <summary>Measure how long an action takes</summary>
		/// <param name="t">An <see cref="Action"/> defining the action to be measured</param>
		/// <returns>The <see cref="TimeSpan"/> it takes to perform the action</returns>
		public static TimeSpan MeasureTime(Action t)
		{
			DateTime startTime = DateTime.Now;
			t(); // perform task
			return DateTime.Now - startTime;
		}
	}
}
