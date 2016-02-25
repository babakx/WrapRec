using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
    public class Item : Entity
    {
        public ICollection<Feedback> Feedbacks { get; private set; }

        public Item(string id)
            : base(id)
        {
            Feedbacks = new HashSet<Feedback>();
        }

        public override string ToString()
        {
            return string.Format("Item{0}, Feedbacks {1}", base.ToString(), Feedbacks.Count);
        }
    }
}
