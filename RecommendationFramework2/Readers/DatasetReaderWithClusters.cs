using RF2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Readers
{
    public abstract class DatasetReaderWithClusters<T> : DatasetReaderWithFilter<T>
    {
        protected Dictionary<string, string> UsersCluster;
        protected Dictionary<string, string> ItemsCluster;
        protected Dictionary<string, string> AuxUsersCluster;
        protected Dictionary<string, string> AuxItemsCluster;
        
        public DatasetReaderWithClusters(string path) 
            : base(path)
        {
            UsersCluster = new Dictionary<string, string>();
            ItemsCluster = new Dictionary<string, string>();
            AuxUsersCluster = new Dictionary<string, string>();
            AuxItemsCluster = new Dictionary<string, string>();
        }

        public DatasetReaderWithClusters(string path, string usersClustersPath, string itemsClusterPath, string auxDomainCode = "")
            : this(path)
        {
            var lines = File.ReadAllLines(usersClustersPath).ToList();

            foreach (string l in lines)
            {
                var parts = l.Split(',');
                UsersCluster.Add(parts[0], "cu" + parts[1]);
            }

            var lines2 = File.ReadAllLines(itemsClusterPath).ToList();

            foreach (string l in lines2)
            {
                var parts = l.Split(',');
                ItemsCluster.Add(parts[0], "ci" + parts[1]);
            }

            if (auxDomainCode != "")
            {
                var lines3 = File.ReadAllLines(usersClustersPath + "." + auxDomainCode).ToList();

                foreach (string l in lines3)
                {
                    var parts = l.Split(',');
                    AuxUsersCluster.Add(parts[0], "acu" + parts[1]);
                }

                var lines4 = File.ReadAllLines(itemsClusterPath + "." + auxDomainCode).ToList();

                foreach (string l in lines4)
                {
                    var parts = l.Split(',');
                    AuxItemsCluster.Add(parts[0], "aci" + parts[1]);
                }
            }
        }
    }
}
