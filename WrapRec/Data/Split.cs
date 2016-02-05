using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.IO;

namespace WrapRec.Data
{
	public enum SplitType
	{ 
		STATIC,
		DYNAMIC,
		CROSSVALIDATION,
		CROSSVALIDATION_SPLIT,
		CUSTOM
	}

	public abstract class Split
	{
		public string Id { get; set; }
		public DataContainer Container { get; set; }
		public Dictionary<string, string> SetupParameters { get; set; }
		public SplitType Type { get; set; }
		public bool IsSetup { get; protected set; }

		protected IEnumerable<Feedback> _train;
        protected IEnumerable<Feedback> _test;

        public IEnumerable<Split> SubSplits { get; set; }

        public IEnumerable<Feedback> Train 
		{ 
			get { return _train; } 
		}

		public IEnumerable<Feedback> Test
		{
			get { return _test; }
		}

		public abstract void Setup();

	}
}
