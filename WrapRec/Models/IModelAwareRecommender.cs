using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Models
{
    public interface IModelAwareRecommender
    {
        Model Model { get; set; }
    }
}
