using Microsoft.EntityFrameworkCore;
using PalletOptimization.Models;
using PalletOptimization.Enums;
using PalletOptimization.Utilities;
using PalletGroup = PalletOptimization.Models.PalletGroup;
using PalletType = PalletOptimization.Models.PalletType;

namespace PalletOptimization.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        //Taking the objects from Model class
        //DbSet is sending a query to database, DBset includes all the values from the database. 
        public DbSet<PalletGroup> PalletGroups { set; get; }
        public DbSet<PalletType> PalletTypes { get; set; }
        public DbSet<Element> Elements { get; set; }

       
    }
    
    }
