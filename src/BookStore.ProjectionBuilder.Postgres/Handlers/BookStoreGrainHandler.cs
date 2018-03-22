using System.Threading.Tasks;
using BookStore.Events.V1.BookStore;
using BookStore.ProjectionBuilder.Postgres.Database;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace BookStore.ProjectionBuilder.Postgres.Handlers
{
    internal class BookStoreGrainHandler : BaseHandler
    {
        private readonly string _streamName;
        private readonly ILogger _log;

        public BookStoreGrainHandler(string streamName)
        {
            _streamName = streamName;
            _log = Log.ForContext<BookStoreGrainHandler>();
        }
        
        protected override async Task HandleInternal(object @event, long streamVersion)
        {
            switch (@event)
            {
                case BookStoreInitializedEvent ev:
                    await Apply(ev, streamVersion);
                    break;
                case BookStoreUpdatedEvent ev:
                    await Apply(ev, streamVersion);
                    break;
                default:
                    _log.Warning("Unknown event type");
                    break;
            }
        }

        private async Task Apply(BookStoreInitializedEvent @event, long version)
        {
            using (var db = new ApplicationDbContext(Configuration.ConnectionString))
            {
                await UpdateStreamVersion(db, version);
                CreateBookStore(db, @event);
                
                await db.SaveChangesAsync();
            }
        }
        
        private async Task Apply(BookStoreUpdatedEvent @event, long version)
        {
            using (var db = new ApplicationDbContext(Configuration.ConnectionString))
            {
                await UpdateStreamVersion(db, version);
                await UpdateBookStore(db, @event);
                
                await db.SaveChangesAsync();
            }
        }

        private void CreateBookStore(ApplicationDbContext db, BookStoreInitializedEvent @event)
        {
            _log.Debug($"Creating book store entity {@event.Id}");
            
            var bookStore = new Database.Entities.BookStore
            {
                Id = @event.Id,
                Name = @event.Name,
                Address = JsonConvert.SerializeObject(@event.Address)
            };
            
            db.BookStores.Add(bookStore);
        }

        private async Task UpdateBookStore(ApplicationDbContext db, BookStoreUpdatedEvent @event)
        {
            _log.Debug($"Updating book store entity {@event.Id}");
            
            var bookStore = await db.BookStores.FindAsync(@event.Id);
            bookStore.Name = @event.Name;
            bookStore.Address = JsonConvert.SerializeObject(@event.Address);

            db.Update(bookStore);
        }
        
        private async Task UpdateStreamVersion(ApplicationDbContext db, long version)
        {
            _log.Debug($"Updating stream version: {version}");
            
            var streamVersionEntity = await db.StreamVersions.FindAsync(_streamName);
            streamVersionEntity.Version = version;
            
            db.Update(streamVersionEntity);
        }
    }
}