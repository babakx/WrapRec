using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Core
{
    public class Entity
    {
        public string Id { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public Entity()
        {
            Attributes = new Dictionary<string, string>();
        }

        public Entity(string id) 
            : this()
        {
            Id = id;
        }
        
        public void AddAttribute(string name, string value)
        {
            // allow multiple entrance into a single property
            if (Attributes.ContainsKey(name))
                Attributes[name] = Attributes[name] + "," + value;
            else
                Attributes.Add(name, value);
        }

        public void UpdateAttribute(string name, string value)
        {
            if (Attributes.ContainsKey(name))
                Attributes[name] = value;
            else
                Attributes.Add(name, value);
        }

        public override string ToString()
        {
            string attributes = Attributes.Count > 0 ? Attributes.Values.Aggregate((a, b) => a + ";" + b) : "";
            
            return string.Format("Id {0}{1}", Id, 
                string.IsNullOrEmpty(attributes) ? "" : ", Attributes: " + attributes);
        }
    }
}
