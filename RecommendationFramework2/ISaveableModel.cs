using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public interface ISaveableModel
    {
        void SaveModel(string path);
        void LoadModel(string path);
    }
}
