using RF2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace RF2.Readers
{
    public class EpinionReader : DatasetReaderWithClusters<EpinionItemRating>
    {
        public EpinionReader(string path) 
            : base(path)
        { }

        public EpinionReader(string path, string usersClustersPath, string itemsClusterPath)
            : base(path, usersClustersPath, itemsClusterPath)
        { }

        public override IEnumerable<EpinionItemRating> ReadWithoutFiltering()
        {
            return File.ReadAllLines(this.Path).Skip(1).Select(l => ParseItemRating(l)).ToList();
        }

        private EpinionItemRating ParseItemRating(string line)
        {
            var parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var ir = new EpinionItemRating();
            ir.User = new MovieLensUser(parts[0]);
            ir.Item = new MovieLensItem(parts[1]);
            ir.Rating = Convert.ToInt32(parts[2]);

            if (UsersCluster.ContainsKey(ir.User.Id))
                ir.UserCluster = UsersCluster[ir.User.Id];

            if (ItemsCluster.ContainsKey(ir.Item.Id))
                ir.ItemCluster = ItemsCluster[ir.Item.Id];

            return ir;
        }
    }
}
