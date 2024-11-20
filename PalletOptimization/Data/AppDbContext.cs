using Microsoft.EntityFrameworkCore;
using PalletOptimization.Models;

namespace PalletOptimization.Data
{
    //Inherits from DbContext which is part of EntityFramework   
    //This class is like a bridge between the database and C#
    public class AppDbContext : DbContext
    {
        //Constructor to initialize DbContext with options
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables for the database
        public DbSet<Elements> Elements { get; set; }
        public DbSet<PalletType> PalletTypes { get; set; }
        public DbSet<PalletGroup> PalletGroups { get; set; }
        
        //OnModelCreating is a method from DbContext which gives us more control over the mapping.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Elements
            //ModelBuilder is responsible for all the mapping. Basically it configures how Elements class maps to the database.
            modelBuilder.Entity<Elements>(entity =>
            {
                entity.HasKey(e => e.Id); //EntityFramework tells SQL Server to create a PK constraint on the Id column in the db.
                //The => means take the object (e) of type (Elements) access its Id and use it as a primary key.
                entity.Property(e => e.Length).IsRequired(); 
                entity.Property(e => e.Width).IsRequired();
                entity.Property(e => e.Height).IsRequired();
                entity.Property(e => e.Weight).IsRequired();
            });

            // Configure PalletType
            modelBuilder.Entity<PalletType>(entity =>
            {
                entity.HasKey(pt => pt.Id); 
                entity.Property(pt => pt.Name)
                    .IsRequired() 
                    .HasMaxLength(50); 
            });

            // Configure PalletGroup
            modelBuilder.Entity<PalletGroup>(entity =>
            {
                entity.HasKey(pg => pg.Id); 
                entity.Property(pg => pg.Length).IsRequired(); 
                entity.Property(pg => pg.Width).IsRequired(); 
                entity.Property(pg => pg.Height).IsRequired(); 
                entity.Property(pg => pg.BaseWeight).IsRequired(); 
                entity.Property(pg => pg.MaxWeight).IsRequired(); 
            });
        }
    }
}
