using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.Relations
{
    public class TouristTrip
    {
        [Key]
        public int bookId { get; set; }

        [ForeignKey(nameof(tourist))]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }

        [ForeignKey(nameof(trip))]
        public int tripId { get; set; }
        public Trip trip { get; set; }
    }
}
