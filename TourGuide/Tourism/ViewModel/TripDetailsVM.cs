using System;
using System.Collections.Generic;

namespace Tourism.ViewModels
{
    public class TripDetailsVM
    {
        public int TripId { get; set; }
        public string TripName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        // Trip timing
        public int Duration { get; set; } // in days
        public DateTime StartDate { get; set; }

        // Calculated end date (StartDate + Duration)
        public DateTime EndDate => StartDate.AddDays(Duration);

        public bool IsFullyBooked { get; set; }
        public byte[] MainImage { get; set; }   // ✅ الصورة الرسمية


        // 🔹 NEW: Capacity and remaining seats
        public int Capacity { get; set; }
        public int RemainingSeats { get; set; }

        // Keep images list (for future gallery or additional images)
        public List<string> Images { get; set; } = new();

        // Tour guide info
        public GuideVM TourGuide { get; set; } = new();

        // Itinerary for the trip
        public List<ItineraryItemVM> Itinerary { get; set; } = new();

        // Booking details for this trip
        public List<BookingDetailVM> Bookings { get; set; } = new();
    }

    public class GuideVM
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class ItineraryItemVM
    {
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
    }

    public class BookingDetailVM
    {
        public string TouristName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        // Optional for future use
        public DateTime BookingDate { get; set; }
    }
}
