namespace Tourism.Models
{
    public class PaymentTripBooking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int TripId { get; set; }

        public int Quantity { get; set; }

        public DateTime BookingDate { get; set; }

        public decimal TotalPrice { get; set; }

        // Navigation
        public Trip Trip { get; set; }


    }


}
