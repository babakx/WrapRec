using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;

namespace WrapRec.Data
{
	public class Split
	{
		public string Id { get; set; }

        public DataContainer Container = DataContainer.GetInstance();
        protected IEnumerable<Feedback> _train;
        protected IEnumerable<Feedback> _test;

        public IEnumerable<Split> SubSplits { get; set; }

        public IEnumerable<Feedback> Train { get { return _train; } }

        public IEnumerable<Feedback> Test { get { return _test; } }

        public void Clear()
        {

        }
	}
}
