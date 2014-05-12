using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class MapEnabledEntity 
    {
        public string Id { get; private set; }

        int _mappedId = -1;

        public MapEnabledEntity(string id)
        {
            Id = id;
        }

        public int GetMappedId(Mapping mapper)
        {
            if (_mappedId == -1)
                _mappedId = mapper.ToInternalID(Id);

            return _mappedId;
        }
    }
}
