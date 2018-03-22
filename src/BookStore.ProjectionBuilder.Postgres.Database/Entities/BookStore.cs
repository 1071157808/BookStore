using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.ProjectionBuilder.Postgres.Database.Entities
{
    public class BookStore
    {
        [Key]
        public Guid Id { get; set; }
        
        [MaxLength(256)]
        public string Name { get; set; }

        [Column(TypeName = "json")]
        public string Address { get; set; }
    }
}