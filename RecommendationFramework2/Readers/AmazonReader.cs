using RF2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Readers
{
    public class AmazonReader : DatasetReaderWithClusters<ItemRatingWithClusters>
    {
        public AmazonReader(string path) 
            : base(path)
        { }

        public AmazonReader(string path, string usersClustersPath, string itemsClusterPath, string auxDomainCode = "")
            : base(path, usersClustersPath, itemsClusterPath, auxDomainCode)
        { }

        public override IEnumerable<ItemRatingWithClusters> ReadWithoutFiltering()
        {
            return File.ReadAllLines(this.Path).Skip(1).Select(l => ParseItemRating(l)).ToList();
        }

        private ItemRatingWithClusters ParseItemRating(string line)
        {
            var parts = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var ir = new ItemRatingWithClusters();
            ir.User = new User(parts[0]);
            ir.Item = new Item(parts[1]);
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
