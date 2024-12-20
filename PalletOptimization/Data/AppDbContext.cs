﻿using Microsoft.EntityFrameworkCore;
using PalletOptimization.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using IdentityDbContext = Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext;

namespace PalletOptimization.Data
{
    // Inherits from DbContext which is part of EntityFramework   
    // This class is like a bridge between the database and C#
    public class AppDbContext : IdentityDbContext
    {
        // Constructor to initialize DbContext with options
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Doesnt contain actual table data until you query it. It just says "there is a table called PalletTypes, use this DbSet to interact with it"
        // If wew run a query "var palletTypes = context.PalletTypes.ToList(); then DbSet will actually contain the data.
        
        // These 2 are read-only tables. We never update them from EF Core, might remove if there is time. 
        public DbSet<Elements> Elements { get; set; }
        public DbSet<PalletType> PalletTypes { get; set; }
        
        // This one is updated via EF Core
        public DbSet<PalletGroup> PalletGroups { get; set; }
        
        //OnModelCreating is a method from DbContext which gives us more control over the mapping.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure Elements
            // ModelBuilder is responsible for all the mapping. Basically it configures how Elements class maps to the database.
            modelBuilder.Entity<Elements>(entity =>
            {
                // We are checking if the Elements table has a key. 
                entity.HasKey(e => e.Id); // EntityFramework tells SQL Server to create a PK constraint on the Id column in the db.
                // The => means take the object (e) of type (Elements) access its Id and use it as a primary key.
                entity.Property(e => e.Name).IsRequired(); 
                entity.Property(e => e.Length).IsRequired(); 
                entity.Property(e => e.Width).IsRequired();
                entity.Property(e => e.Height).IsRequired();
                entity.Property(e => e.Weight).IsRequired();
                entity.Property(e => e.HeightWidthFactor)
                    .HasColumnType("float")
                    .IsRequired()
                    .HasDefaultValue(1.0);
                
                //store rotation rules as int
                entity.Property(e => e.RotationRules)
                    .HasConversion<int>() // Ensure the enum is stored as an int
                    .IsRequired();
                
                // Map IsSpecial to bit (boolean)
                entity.Property(e => e.IsSpecial)
                    .HasColumnType("bit")
                    .IsRequired();
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
                entity.Property(pg => pg.Name).IsRequired();
                entity.Property(pg => pg.Length).IsRequired(); 
                entity.Property(pg => pg.Width).IsRequired(); 
                entity.Property(pg => pg.Height).IsRequired(); 
                entity.Property(pg => pg.BaseWeight).IsRequired(); 
                entity.Property(pg => pg.MaxWeight).IsRequired();
            });
        }
        
        // Override SaveChanges to make Elements and PalletType read-only
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Elements || entry.Entity is PalletType)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Unchanged;
                    }
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Elements || entry.Entity is PalletType)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Unchanged;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
    

