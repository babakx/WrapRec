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

        public ICollection<Attribute> Attributes { get; protected set; }
		public Dictionary<string, Attribute> Attribute { get; protected set; }

        public Entity()
        {
			Attributes = new List<Attribute>();
        }

        public Entity(string id) 
            : this()
        {
            Id = id;
        }

		// TODO this is a temporary solution, the List should be replaced by dictionary
		public void SetupAttributeDic()
		{
			Attribute = Attributes.ToDictionary(f => f.Name, f => f);
		}

		public Attribute GetOrCreateAttribute(string name)
		{
			var attr = Attributes.FirstOrDefault(a => a.Name == name);

			if (attr != null)
				return attr;
			
			attr = new Attribute() { Name = name };
			Attributes.Add(attr);
			return attr;
		}

        public override string ToString()
        {
            string attributes = Attributes.Count > 0 ? Attributes.Select(a => a.Name + ":" + a.Value)
				.Aggregate((a, b) => a + " " + b) : "";
            
            return string.Format("Id {0}{1}", Id, 
                string.IsNullOrEmpty(attributes) ? "" : ", Attributes " + attributes);
        }
    }
}
