using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainTables.Models;

namespace DomainTables.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(20)]
        public string ISBN { get; set; }

        public DateTime PublishedDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [ForeignKey("Author")]
        public int AuthorId { get; set; }

        [ForeignKey("Genre")]
        public int GenreId { get; set; }

        // Navigation properties
        public virtual Author Author { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();
    }
}
