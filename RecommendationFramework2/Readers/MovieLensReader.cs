using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RF2.Entities;
using LinqLib.Sequence;

namespace RF2.Readers
{
    public class MovieLensReader : DatasetReaderWithClusters<MovieLensItemRating>
    {
        public MovieLensReader(string path) 
            : base(path)
        { }

        public MovieLensReader(string path, string usersClustersPath, string itemsClusterPath, string auxDomainCode = "")
            : base(path, usersClustersPath, itemsClusterPath, auxDomainCode)
        { }

        public override IEnumerable<MovieLensItemRating> ReadWithoutFiltering()
        {
            return File.ReadAllLines(this.Path).Select(l => ParseItemRating(l)).ToList();
        }

        private MovieLensItemRating ParseItemRating(string line)
        {
            var parts = line.Split(new string[] { "::", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

            var ir = new MovieLensItemRating();
            ir.User = new MovieLensUser(parts[0]);
            ir.Item = new MovieLensItem(parts[1]);
            ir.Rating = Convert.ToInt32(parts[2]);

            if (UsersCluster.ContainsKey(ir.User.Id))
                ir.UserCluster = UsersCluster[ir.User.Id];

            if (ItemsCluster.ContainsKey(ir.Item.Id))
                ir.ItemCluster = ItemsCluster[ir.Item.Id];

            if (AuxUsersCluster.ContainsKey(ir.User.Id))
                ir.AuxUserCluster = AuxUsersCluster[ir.User.Id];

            if (AuxItemsCluster.ContainsKey(ir.Item.Id))
                ir.AuxItemCluster = AuxItemsCluster[ir.Item.Id];

            return ir;
        }
    }
}
