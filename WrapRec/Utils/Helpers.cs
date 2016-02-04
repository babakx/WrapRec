using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WrapRec.Utils
{
	public static class Helpers
	{
		public static Type ResolveType(string typeName)
		{ 
			var parts = typeName.Split(':');

			// check if type has an assembly associated with it
			if (parts.Length == 2)
			{
				Assembly asm = Assembly.LoadFrom(parts[0]);
				return asm.GetType(parts[1], true, true);
			}

			return Type.GetType(parts[0], true);
		}
	}
}
