using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.ViewModel;

namespace Tourism.IRepository
{
    public interface ITouristRepository
    {
        // ---------- Synchronous helpers ----------
        public Tourist GetByEmail(string email);
        
        // Earnings / analytics
        public double TotalEearningsRestaurants();
        public double TotalEearningsProducts();
        public double TotalEearningsHotels();
        public double TotalEearningsTrips();
        
        decimal MonthlyEarningsProducts(int month, int merchantId);
        int GetSumAmountProducts(int days, int merchantId);
        double GetSumPriceProducts(int days, int merchantId);

        // ---------- Business operations ----------
        public bool AddToCart(int productId, int touristId);
        Task<bool> AddToCartAsync(int productId, int touristId);
        
        public void transaction(CreditCard cardfromrequest, decimal money, string operation,int id);
        Task TransactionAsync(CreditCard cardFromRequest, decimal money, string operation, int id);
        
        public void BuyProduct(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product,string location);
        Task BuyProductAsync(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product);
        
        public void RemoveFromCart(int productId, int touristId);
        public void CancelOrder(int orderId);
        public List<ProductOrdersViewModel> GetOrders(int touristId);

        public bool review(int touristId, int serviceId, int rate, string comment, string type);

        // ---------- Tourist & Trip lookups ----------
        Task<Tourist?> GetTouristByEmailAsync(string email);
        Task<Trip?> GetTripByIdAsync(int id);

        // ---------- Save / Persistence ----------
        Task SaveChangesAsync();
        Task SaveAsync();

        // ---------- Trip Cart Operations ----------
        Task<TouristCart?> GetCartItemAsync(int touristId, int tripId);
        Task<decimal> GetCartTotalAsync(int touristId);
        Task AddCartItemAsync(TouristCart item);
        Task UpdateCartItemAsync(TouristCart item);
        Task SaveCartAsync(int touristId, List<TouristCart> items);
        Task<List<TouristCart>> GetCartAsync(int touristId);
        Task<int> GetCartCountAsync(int touristId);
        Task<(int quantity, decimal cartTotal)> ChangeCartQuantityAsync(int touristId, int tripId, int delta);
        Task ClearCartAsync(int touristId);
        Task RemoveCartItemAsync(TouristCart cartItem);
        Task<TouristCart?> GetCartItemByIdAsync(int cartItemId);

        Task<IEnumerable<TouristCart>> GetCartItemsWithTripAsync(string userId);
        Task RemoveCartItemsAsync(IEnumerable<TouristCart> items);

        // ---------- Credit Card ----------
        Task<CreditCard?> GetCreditCardAsync(string number, string cvv, string holder, string expiry);
        Task UpdateCreditCardAsync(CreditCard card);

        // ---------- Booking ----------
        Task AddBookingAsync(PaymentTripBooking booking);

        // ---------- Transactions support ----------
        bool SupportsTransactions { get; }
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Add money to Tour Guide's Card
        Task CreditTourGuideAsync(int tourGuideId, decimal amount);
    }
}
