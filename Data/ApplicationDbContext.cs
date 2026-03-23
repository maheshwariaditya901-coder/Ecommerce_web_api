using Ecommerce_web_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_api.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Seller> Sellers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between User and Seller
            modelBuilder.Entity<User>()
                .HasOne(u => u.Seller)
                .WithOne(s => s.User)
                .HasForeignKey<Seller>(s => s.UserId);
        }

        public DbSet<Products> Products { get; set; }
    }
}
