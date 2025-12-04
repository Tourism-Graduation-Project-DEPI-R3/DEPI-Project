using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class TouristProduct
    {
        [Required]
        [ForeignKey(nameof(tourist))]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }

        [Required]
        [ForeignKey(nameof(product))]
        public int productId { get; set; }
        public Product product { get; set; }

        [Key]
        public int orderId { get; set; }

        [Range(1, int.MaxValue)]
        public int amount { get; set; }

        public DateTime orderDate { get; set; } = DateTime.UtcNow;

        public DateTime? arrivalDate { get; set; }

        public bool status { get; set; }   // 1 transaction happened to the merchant. 0 didn't happen
    }
}
