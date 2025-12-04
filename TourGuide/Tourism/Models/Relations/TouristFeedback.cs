using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class TouristFeedback
    {
        [Key]
        public int id { get; set; }

        [Required]
        [ForeignKey(nameof(Tourist))]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }

        [Range(1, 5)]
        public int rating { get; set; }

        public string? comment { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        // Polymorphic target — cannot FK annotate since it's polymorphic
        [Required]
        public int targetId { get; set; }

        [Required]
        public string targetType { get; set; }
    }

 
}
