using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserdataManagement.Models;

namespace UserdataManagement.Data
{
    public class UserDataDbContext : DbContext
    {
        public UserDataDbContext(DbContextOptions<UserDataDbContext> options) : base(options) { }

        public DbSet<UserDataModel> UserData { get; set; }
        public DbSet<VisitorModel> Visitors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Specify the table name for UserDataModel
            modelBuilder.Entity<UserDataModel>().ToTable("userdata");
            modelBuilder.Entity<VisitorModel>().ToTable("visitors");
        }
    }
}
