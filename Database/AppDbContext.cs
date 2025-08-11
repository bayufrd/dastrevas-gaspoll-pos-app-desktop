using System;
using KASIR.Database.ModalDatabase;
using Microsoft.EntityFrameworkCore;

namespace KASIR.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<dbCartModal> Carts { get; set; } // Untuk Cart Modal
        public DbSet<dbCartDetails> CartItems { get; set; } // Untuk Cart Details
        public DbSet<dbRefundDetails> Refunds { get; set; } // Untuk Refunds

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!Directory.Exists("Database")) { Directory.CreateDirectory("Database"); }
            string dbPath = "Database/gaspol.db";
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<dbCartModal>()
                .HasMany(c => c.CartDetails)
                .WithOne()
                .HasForeignKey(cd => cd.CartDetailId); // Mengaitkan detail cart dengan Cart modal

            modelBuilder.Entity<dbCartModal>().ToTable("Carts");
            modelBuilder.Entity<dbCartDetails>().ToTable("CartItems");
            modelBuilder.Entity<dbRefundDetails>().ToTable("Refunds");
        }
    }
}
