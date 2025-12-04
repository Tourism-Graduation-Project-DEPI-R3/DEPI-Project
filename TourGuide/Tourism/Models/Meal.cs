using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models
{
    public class Meal
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(20)]
        public string name { get; set; }
        [Required]
        [StringLength(30)]
        public string description { get; set; }
        [Required]
        public double price { get; set; }
        [Required]
        public byte[] pic { get; set; }
        [ForeignKey("Restaurant")]
        public int restaurantId { get; set; }
        public Restaurant restaurant { get; set; }

        [Required]
        public DateTime dateAdded { get; set; }
        public bool accepted { get; set; } = false;

    }
}
