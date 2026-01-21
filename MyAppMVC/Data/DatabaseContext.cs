using Microsoft.EntityFrameworkCore;
using MyAppMVC.Models;

namespace MyAppMVC.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemClient>().HasKey(ic => new
            {
                ic.ItemId,
                ic.ClientId
            });

            modelBuilder.Entity<ItemClient>()
                .HasOne<Item>(ic => ic.Item)
                .WithMany(i => i.ItemClients)
                .HasForeignKey(ic => ic.ItemId);

            modelBuilder.Entity<ItemClient>()
                .HasOne<Client>(ic => ic.Client)
                .WithMany(c => c.ItemClients)
                .HasForeignKey(ic => ic.ClientId);
        }

        public DbSet<MyAppMVC.Models.Item> Items { get; set; }
        public DbSet<MyAppMVC.Models.SerialNumber> SerialNumbers { get; set; }
        public DbSet<MyAppMVC.Models.Category> Categories { get; set; }
        public DbSet<MyAppMVC.Models.Client> Clients { get; set; }
        public DbSet<MyAppMVC.Models.ItemClient> ItemClients { get; set; }
        public DbSet<MyAppMVC.Models.Supplier> Suppliers { get; set; }
        public DbSet<MyAppMVC.Models.PurchaseOrderItem> PurcahseOrderItems { get; set; }
        public DbSet<MyAppMVC.Models.PurchaseOrder> PurchaseOrders { get; set; }
    }
}
