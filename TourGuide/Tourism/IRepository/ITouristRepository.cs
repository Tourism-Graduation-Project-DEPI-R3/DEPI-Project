using System.Collections.Generic;
using System.Threading.Tasks;
using Tourism.Models;
using Tourism.ViewModel;

namespace Tourism.IRepository
{
    public interface ITouristRepository
    {
        // ---------- Synchronous helpers ----------
        Tourist GetByEmail(string email);

        // Earnings / analytics
        double TotalEearningsRestaurants();
        double TotalEearningsProducts();
        double TotalEearningsHotels();
        double TotalEearningsTrips();

        decimal MonthlyEarningsProducts(int month, int merchantId);
        int GetSumAmountProducts(int days, int merchantId);
        double GetSumPriceProducts(int days, int merchantId);

        // ---------- Business operations ----------
        Task<bool> AddToCartAsync(int productId, int touristId);
        Task TransactionAsync(CreditCard cardFromRequest, decimal money, string operation, int id);

        Task BuyProductAsync(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product);
        void BuyProduct(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product);

        // ---------- Tourist & Trip lookups ----------
        Task<Tourist?> GetTouristByEmailAsync(string email);
        Task<Trip?> GetTripByIdAsync(int id);

        // ---------- Save / Persistence ----------
        Task SaveChangesAsync();
        Task SaveAsync();

        // ---------- Cart ----------
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

        // ✅ NEW METHOD: Add money to Tour Guide's Card
        Task CreditTourGuideAsync(int tourGuideId, decimal amount);

    }
}