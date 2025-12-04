namespace Tourism.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int touristsCount { get; set; }
        public int restaurantsCount { get; set; }
        public int hotelsCount { get; set; }
        public int tourGuidesCount { get; set; }
        public int merchantsCount { get; set; }
        public double earningsFromEcommerce { get; set; }
        public double earningsFromTrips { get; set; }
        public double earningsFromHotels { get; set; }
        public double earningsFromRestaurants { get; set; }
        public double totalEarnings => earningsFromEcommerce + earningsFromTrips + earningsFromRestaurants + earningsFromHotels;
    }
}
