using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class EntityRelation : Entity
    {
        public EntityRelation(string id)
            : base(id)
        { }

        public EntityRelation()
            : this ("-1")
        { }
    }
}
