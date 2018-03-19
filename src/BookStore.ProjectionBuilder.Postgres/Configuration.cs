using System.IO;
using Microsoft.Extensions.Configuration;

namespace BookStore.ProjectionBuilder.Postgres
{
    public static class Configuration
    {
        static Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Instance = builder.Build();
        }

        public static IConfiguration Instance { get; }
    }
}