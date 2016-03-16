using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
	public enum AttributeType
	{ 
		Binary,
		Discrete,
		RealValued
	}
	
	public class Attribute
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public AttributeType Type { get; set; }

		public Tuple<int, float> Translation { get; set; }
	}
}
