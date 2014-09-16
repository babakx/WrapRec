using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;

namespace WrapRec
{
    public class RankedList<T>
    {
        Dictionary<T, float> _items;

        public RankedList()
        {
            _items = new Dictionary<T, float>();
        }

        public float this[T item]
        {
            get
            {
                return _items[item];
            }
            set
            {
                _items[item] = value;
            }
        }

        public Dictionary<T, float> Items
        {
            get { return _items; }
        }
    }
}
