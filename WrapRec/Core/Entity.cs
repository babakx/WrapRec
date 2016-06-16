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

        public Dictionary<string, Attribute> Attributes { get; protected set; }

        public Entity()
        {
			Attributes = new Dictionary<string, Attribute>();
        }

        public Entity(string id) 
            : this()
        {
            Id = id;
        }

		public Attribute GetOrCreateAttribute(string name)
		{
			if (Attributes.ContainsKey(name))
		        return Attributes[name];

			var attr = new Attribute() { Name = name };
			Attributes.Add(name, attr);

			return attr;
		}

        public override string ToString()
        {
            string attributes = Attributes.Count > 0 ? Attributes.Values.Select(a => a.Name + ":" + a.Value)
				.Aggregate((a, b) => a + " " + b) : "";
            
            return string.Format("Id {0}{1}", Id, 
                string.IsNullOrEmpty(attributes) ? "" : ", Attributes " + attributes);
        }
    }
}
