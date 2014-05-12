using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RF2.Entities
{
    public class MovieLensItemRating : ItemRatingWithClusters
    {
        public MovieLensItemRating()
        { }
        
        public MovieLensItemRating(string line)
        {
            string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            this.User = new MovieLensUser(tokens[0]);
            this.Item = new MovieLensItem(tokens[1]);
            this.Rating = Convert.ToInt32(tokens[2]);
        }

        public MovieLensItemRating(string line, string userClusterId, string itemClusterId)
            : this(line)
        {
            UserCluster = userClusterId;
            ItemCluster = itemClusterId;
        }

    }
}
