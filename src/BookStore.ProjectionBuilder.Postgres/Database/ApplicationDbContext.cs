using BookStore.ProjectionBuilder.Postgres.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookStore.ProjectionBuilder.Postgres.Database
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            var connectionString = Configuration.Instance.GetConnectionString(nameof(ApplicationDbContext));
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<StreamVersion> StreamVersions { get; set; }

        public DbSet<Entities.BookStore> BookStores { get; set; }
    }
}