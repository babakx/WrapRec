using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;

namespace MusicRec
{
    public class UserSession
    {
        public User User { get; set; }
        public IList<Item> Items { get; private set; }

        public UserSession()
        {
            Items = new List<Item>();
        }
    }
}
