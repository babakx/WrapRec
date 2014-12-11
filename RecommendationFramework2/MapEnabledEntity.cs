using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class Entity 
    {
        public string Id { get; set; }

        int _mappedId = -1;

        public double? Timestamp { get; set; }

        public Dictionary<string, string> Properties { get; private set; }

        public Entity(string id)
        {
            Id = id;
            Properties = new Dictionary<string, string>();
        }

        public void AddProperty(string name, string value)
        {
            Properties.Add(name, value);
        }

        public int GetMappedId(Mapping mapper)
        {
            if (_mappedId == -1)
                _mappedId = mapper.ToInternalID(Id);

            return _mappedId;
        }

        public string GetProperty(string name)
        {
            string value = "";
            Properties.TryGetValue(name, out value);

            return value;
        }
    }
}
