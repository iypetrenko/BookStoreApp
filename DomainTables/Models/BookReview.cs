using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DomainTables.Models
{
    public class BookReview
    {
        [Key]
        public int ReviewId { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReviewerName { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; }

        // Navigation property
        public virtual Book Book { get; set; }
    }
}