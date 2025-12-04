namespace Tourism.ViewModels
{
    public class BookingItemVM
    {
        public int TouristId { get; set; }
        public string TouristName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class TripOrdersVM
    {
        public int TripId { get; set; }
        public string TripName { get; set; }
        public string Destination { get; set; }
        public decimal Cost { get; set; }
        public int TotalBookingsCount { get; set; }
        public List<BookingItemVM> Bookings { get; set; } = new();
    }
}
