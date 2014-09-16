using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class RatingHistogram5
    {
        public string ItemId { get; set; }

        public int R1 { get; set; }
        public int R2 { get; set; }
        public int R3 { get; set; }
        public int R4 { get; set; }
        public int R5 { get; set; }

        public double[] ToDoubleArray()
        {
            return new double[] { R1, R2, R3, R4, R5 };
        }
    }
}
