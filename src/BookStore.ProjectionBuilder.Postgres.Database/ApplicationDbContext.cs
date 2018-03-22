using BookStore.ProjectionBuilder.Postgres.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.ProjectionBuilder.Postgres.Database
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(_connectionString);
        }

        public DbSet<StreamVersion> StreamVersions { get; set; }

        public DbSet<Entities.BookStore> BookStores { get; set; }
    }
}