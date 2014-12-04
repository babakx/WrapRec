using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class LibFmProcessor
    {
        public string DataFile { get; set; }
        
        public LibFmProcessor(string dataFile)
        {
            DataFile = dataFile;
        }

        public void Process()
        {
            ReorderLines();
        }

        private void ReorderLines()
        { 

        }
    }
}
