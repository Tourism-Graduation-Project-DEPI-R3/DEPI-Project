using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class TouristRestaurant
    {
        [ForeignKey("Tourist")]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }
        [ForeignKey("Restaurant")]
        public int restaurant_id { get; set; }
        public Restaurant restaurant { get; set; }


        [Key]
        public int bookId { get; set; }
        public int tableNumber { get; set; }
        public int numberOfGuests { get; set; }
        public DateTime date { get; set; }
    }
}
