using WrapRec.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Readers
{
    public class AmazonReader : DatasetReaderWithClusters<ItemRatingWithClusters>
    {


        public AmazonReader(string path) 
            : base(path)
        { }

        public AmazonReader(string path, string usersClustersPath, string itemsClusterPath, string auxDomainCode = "", bool randomClusters = false)
            : base(path, usersClustersPath, itemsClusterPath, auxDomainCode, randomClusters)
        {
        }

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

            int uc = 0, ic = 0, uca = 0, ica = 0;

            if (RandomClusters)
            {
                Random r = new Random();
                double u = r.Next() * ClusterNumbers;
                double i = r.Next() * ClusterNumbers;
                double ua = r.Next() * ClusterNumbers;
                double ia = r.Next() * ClusterNumbers;
                
                uc = Convert.ToInt32(Math.Round(u));
                ic = Convert.ToInt32(Math.Round(i));
                uca = Convert.ToInt32(Math.Round(ua));
                ica = Convert.ToInt32(Math.Round(ia));
            }

            if (UsersCluster.ContainsKey(ir.User.Id))
                ir.UserCluster = RandomClusters ? uc.ToString() : UsersCluster[ir.User.Id];

            if (ItemsCluster.ContainsKey(ir.Item.Id))
                ir.ItemCluster = RandomClusters ? ic.ToString() : ItemsCluster[ir.Item.Id];

            if (AuxUsersCluster.ContainsKey(ir.User.Id))
                ir.AuxUserCluster = RandomClusters ? uca.ToString() : AuxUsersCluster[ir.User.Id];

            if (AuxItemsCluster.ContainsKey(ir.Item.Id))
                ir.AuxItemCluster = RandomClusters ? ica.ToString() : AuxItemsCluster[ir.Item.Id];

            return ir;
        }
    }
}
