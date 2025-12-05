using Tourism.Models;
using Tourism.Models.Relations;

namespace Tourism.ViewModel
{
    public class MyBookingsViewModel
    {
        public IEnumerable<TouristRoom> RoomBookings { get; set; } = new List<TouristRoom>();
        public IEnumerable<TripBookingViewModel> TripBookings { get; set; } = new List<TripBookingViewModel>();
    }

    public class TripBookingViewModel
    {
        public int BookingId { get; set; }
        public int TripId { get; set; }
        public string TripName { get; set; }
        public string Destination { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public byte[]? MainImage { get; set; }
        public int TourGuideId { get; set; }
    }
}
