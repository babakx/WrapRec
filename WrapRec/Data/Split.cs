using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;

namespace WrapRec.Data
{
	public class Split : ISplit
	{
		public string Id { get; set; }

        public IEnumerable<Split> SubSplits { get; protected set; }

        public IEnumerable<Feedback> Train { get; protected set; }

        public IEnumerable<Feedback> Test { get; protected set; }

        public void Clear()
        {

        }
	}
}
