using System.IO;
using Microsoft.Extensions.Configuration;

namespace BookStore.ProjectionBuilder.Postgres
{
    public static class Configuration
    {
        private static readonly IConfiguration _config;
        
        static Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _config = builder.Build();
        }

        public static string ConnectionString => _config.GetConnectionString("bookstore_projection");
    }
}