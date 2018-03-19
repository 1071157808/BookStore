using System.Linq;
using System.Threading.Tasks;
using BookStore.ProjectionBuilder.Postgres.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Serilog;

namespace BookStore.ProjectionBuilder.Postgres.Database
{
    public static class StreamVersionsManager
    {
        private static readonly ILogger _log;

        static StreamVersionsManager()
        {
            _log = Log.ForContext(typeof(StreamVersionsManager));
        }
        
        public static async Task<(string StreamName, long Version)[]> GetStreamVersions()
        {
            using (var db = new ApplicationDbContext())
            {
                _log.Debug("Reading streams versions from db.");
                var result = await db.StreamVersions.ToArrayAsync();
                _log.Debug($"Found {result.Length} streams version(s) in db.");
                
                return result.Select(i => (i.StreamName, i.Version)).ToArray();
            }
        }

        public static async Task InitializeStreamVersions(string[] streamNames)
        {
            using (var db = new ApplicationDbContext())
            {
                _log.Debug("Reading streams versions from db.");
                var existingStreams = await db.StreamVersions.Select(i => i.StreamName).ToArrayAsync();
                _log.Debug($"Found {existingStreams.Length} streams version(s) in db.");
                
                var streamVersionsToInsert = streamNames.Except(existingStreams)
                    .Select(i => new StreamVersion {StreamName = i, Version = 0}).ToArray();

                if (streamVersionsToInsert.Any())
                {
                    _log.Debug($"Adding {streamVersionsToInsert.Select(i => $"{i.StreamName}").Join()} streams version(s) into db.");
                    await db.StreamVersions.AddRangeAsync(streamVersionsToInsert);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}