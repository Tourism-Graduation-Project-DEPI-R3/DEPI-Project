using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models
{
    public class Table
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int number { get; set; }
        [Required]
        public int numberOfPersons { get; set; }
        [Required]
        public double bookingPrice { get; set; }
        [Required]
        public bool status { get; set; }
        [ForeignKey("Restaurant")]
        public int restaurant_id { get; set; }
        public Restaurant restaurant { get; set; }
        public bool accepted { get; set; } = false;

        
        public DateTime? dateAdded { get; set; }

    }
}
