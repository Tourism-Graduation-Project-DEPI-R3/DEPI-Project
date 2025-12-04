using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class TouristRoom
    {
        [Key]
        public int bookId { get; set; }

        [ForeignKey(nameof(tourist))]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }

        [ForeignKey(nameof(room))]
        public int roomId { get; set; }
        public Room room { get; set; }

        public int numberOfGuests { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
