using System.ComponentModel.DataAnnotations;

namespace BookStore.ProjectionBuilder.Postgres.Database.Entities
{
    public class StreamVersion
    {
        [Key]
        [MaxLength(64)]
        public string StreamName { get; set; }

        public long Version { get; set; }
    }
}