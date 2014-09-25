using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Samples.EntityDataReader;

namespace WrapRec.Data
{
    public class RecSysContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<Relation> Relations { get; set; }

        public RecSysContext()
            : base("RecSysConnectionString")
        {
             Database.SetInitializer<RecSysContext>(new DropCreateDatabaseIfModelChanges<RecSysContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => new { r.UserId, r.DatasetId });

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.Item)
                .WithMany(i => i.Ratings)
                .HasForeignKey(r => new { r.ItemId, r.DatasetId });

            modelBuilder.Entity<Relation>()
                .HasRequired(c => c.User)
                .WithMany(u => u.ConnectedTo)
                .HasForeignKey(c => new { c.UserId, c.DatasetId });

            modelBuilder.Entity<Relation>()
                .HasRequired(c => c.ConnectedUser)
                .WithMany(u => u.IsConnectedBy)
                .HasForeignKey(c => new { ConnectedId = c.ConnectedId, c.DatasetId });
            
            // workaround to solve forgeing key cycle problem
            // see: http://stackoverflow.com/questions/14489676
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public void ImportToTable<T>(string tableName, IEnumerable<T> data)
        {
            var bulkCopy = new SqlBulkCopy(ConnectionString);
            bulkCopy.BulkCopyTimeout = 600;

            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(data.AsDataReader());
        }

        public void ImportItemRatings(IEnumerable<ItemRating> itemRatings, Dataset datasetRecord)
        {
            // step 1: import users
            var users = itemRatings.Select(ir => ir.User.Id).Distinct()
                .Select(uId =>
                {
                    User u = new User();
                    u.Id = uId;
                    u.DatasetId = datasetRecord.Id;

                    return u;
                });

            Console.WriteLine("Importing {0} users ...", users.Count());
            ImportToTable("Users", users);

            // step 2: importing items
            var items = itemRatings.Select(ir => ir.Item.Id).Distinct()
                .Select(iId =>
                {
                    Item i = new Item();
                    i.Id = iId;
                    i.DatasetId = datasetRecord.Id;

                    return i;
                });

            Console.WriteLine("Importing {0} items ...", items.Count());
            ImportToTable("Items", items);

            // step 3: importing ratings
            var ratings = itemRatings.Select(ir =>
            {
                Rating r = new Rating();
                r.UserId = ir.User.Id;
                r.ItemId = ir.Item.Id;
                r.DatasetId = datasetRecord.Id;
                r.Rate = ir.Rating;

                return r;
            });

            Console.WriteLine("Importing {0} Ratings...", ratings.Count());
            ImportToTable("Ratings", ratings);
        }

        public string ConnectionString 
        { 
            get 
            {
                return ConfigurationManager.ConnectionStrings["RecSysConnectionString"].ConnectionString;
            } 
        }
    }
}
