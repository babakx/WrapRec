using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Data;

namespace MusicRec
{
    public class MusicDataContainer : DataContainer
    {
        public ICollection<UserSession> Sessions { get; private set; }

        public MusicDataContainer()
            : base()
        {
            Sessions = new HashSet<UserSession>();
        }
    }
}
