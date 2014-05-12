using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class Domain
    {
        public string Id { get; set; }
        public float Weight { get; set; }
        public IDatasetReader<ItemRating> RatingsReader { get; set; }

        public Domain(string id, IDatasetReader<ItemRating> ratingsReader)
            : this(id, ratingsReader, 1)
        { }

        public Domain(string id, IDatasetReader<ItemRating> ratingsReader, float weight)
        {
            Id = id;
            RatingsReader = ratingsReader;
            Weight = weight;
        }

    }
}
