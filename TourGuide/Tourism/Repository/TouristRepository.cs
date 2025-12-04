using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.Repository
{
    public class TouristRepository : ITouristRepository
    {
        private readonly TourismDbContext _context;
        private IDbContextTransaction? _transaction;

        public TouristRepository(TourismDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Tourist GetByEmail(string email)
        {
            return _context.Tourists.FirstOrDefault(t => t.email == email);
        }

        // ... Earnings methods omitted for brevity ...
        public double TotalEearningsRestaurants() => _context.Tables.Join(_context.TouristRestaurants, tb => new { tb.restaurant_id, tb.number }, tr => new { tr.restaurant_id, number = tr.tableNumber }, (tb, tr) => (double?)tb.bookingPrice).Sum() ?? 0;
        public double TotalEearningsProducts() => _context.TouristProducts.Where(tp => tp.status == true).Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => (double?)(p.price * tp.amount)).Sum() ?? 0;
        public double TotalEearningsHotels() => _context.TouristRooms.Join(_context.Rooms, tr => tr.roomId, r => r.id, (tr, r) => (double?)r.cost).Sum() ?? 0;
        public double TotalEearningsTrips() => _context.TouristCarts.Select(c => (double?)c.TotalPrice).Sum() ?? 0;

        public decimal MonthlyEarningsProducts(int month, int merchantId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, month, 1);
            var monthEnd = month == 12 ? new DateTime(today.Year + 1, 1, 1) : new DateTime(today.Year, month + 1, 1);
            return _context.TouristProducts.Where(tp => tp.status == true && tp.orderDate >= monthStart && tp.orderDate < monthEnd)
                .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId)
                .Sum(x => (decimal?)x.p.price * x.tp.amount) ?? 0;
        }

        public int GetSumAmountProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return _context.TouristProducts.Where(tp => tp.status == true && tp.orderDate >= startDate && tp.orderDate < today.AddDays(1))
                .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId).Sum(x => x.tp.amount);
        }

        public double GetSumPriceProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return _context.TouristProducts.Where(tp => tp.status == true && tp.orderDate >= startDate && tp.orderDate < today.AddDays(1))
                 .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId).Sum(x => (double?)x.p.price * x.tp.amount) ?? 0;
        }

        // -------------------- PRODUCT CART / PURCHASE --------------------
        public async Task<bool> AddToCartAsync(int productId, int touristId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;
            var tourist = await _context.Tourists.FindAsync(touristId);
            if (tourist == null) return false;

            var existingCartItem = await _context.CartProducts.FirstOrDefaultAsync(c => c.ProductId == productId && c.TouristId == touristId);
            if (existingCartItem == null)
            {
                var cp = new CartProduct { ProductId = productId, Product = product, TouristId = touristId, Tourist = tourist };
                await _context.CartProducts.AddAsync(cp);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task TransactionAsync(CreditCard cardFromRequest, decimal money, string operation, int id)
        {
            var tourist = await _context.Tourists.FirstOrDefaultAsync(x => x.id == id);
            if (tourist == null) throw new InvalidOperationException("Tourist not found.");

            if (operation == "withdraw") { cardFromRequest.Balance += money; tourist.balance -= (double)money; }
            else { cardFromRequest.Balance -= money; tourist.balance += (double)money; }

            _context.CreditCards.Update(cardFromRequest);
            _context.Tourists.Update(tourist);
            await _context.SaveChangesAsync();
        }

        public async Task BuyProductAsync(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product)
        {
            var tourist = await _context.Tourists.FindAsync(touristId);
            if (tourist == null) throw new InvalidOperationException("Tourist not found.");
            decimal totalPrice = (decimal)(product.price * product.count);

            if (buyCredit) { if (cc == null) throw new ArgumentNullException(nameof(cc)); cc.Balance -= totalPrice; _context.CreditCards.Update(cc); }
            else { tourist.balance -= (double)totalPrice; _context.Tourists.Update(tourist); }

            var prod = await _context.Products.FindAsync(product.productId);
            var tp = new TouristProduct { product = prod, tourist = tourist, productId = (int)product.productId, touristId = touristId, amount = product.count, status = false, arrivalDate = DateTime.Today.AddDays((int)product.DeliversWithin) };
            await _context.TouristProducts.AddAsync(tp);

            await _context.InboxMsgs.AddAsync(new InboxMsg { providerType = "Merchant", content = $"{tourist.firstName} has ordered {product.count} unit(s) of {product.name}.", providerId = (int)product.id, date = DateTime.Today });

            var cp = await _context.CartProducts.FirstOrDefaultAsync(c => c.ProductId == product.productId && c.TouristId == touristId);
            if (cp != null) _context.CartProducts.Remove(cp);
            await _context.SaveChangesAsync();
        }

        public void BuyProduct(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product) => BuyProductAsync(touristId, buyCredit, cc, product).GetAwaiter().GetResult();

        // -------------------- TRIP CART --------------------
        public async Task<TouristCart?> GetCartItemAsync(int touristId, int tripId) => await _context.TouristCarts.FirstOrDefaultAsync(c => c.TouristId == touristId && c.TripId == tripId);
        public async Task<decimal> GetCartTotalAsync(int touristId) => await _context.TouristCarts.Where(c => c.TouristId == touristId).SumAsync(c => c.TotalPrice);
        public async Task AddCartItemAsync(TouristCart item) => await _context.TouristCarts.AddAsync(item);
        public Task UpdateCartItemAsync(TouristCart item) { _context.TouristCarts.Update(item); return Task.CompletedTask; }

        public async Task SaveCartAsync(int touristId, List<TouristCart> items)
        {
            foreach (var item in items)
            {
                var existing = await _context.TouristCarts.FirstOrDefaultAsync(c => c.TouristId == touristId && c.TripId == item.TripId);
                if (existing == null) { item.TouristId = touristId; await _context.TouristCarts.AddAsync(item); }
                else { existing.Quantity = item.Quantity; existing.UnitPrice = item.UnitPrice; existing.TotalPrice = item.TotalPrice; _context.TouristCarts.Update(existing); }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<TouristCart>> GetCartAsync(int touristId) => await _context.TouristCarts.Where(c => c.TouristId == touristId).Include(c => c.Trip).ToListAsync();
        public async Task<int> GetCartCountAsync(int touristId) => await _context.TouristCarts.Where(c => c.TouristId == touristId).SumAsync(c => c.Quantity);

        public async Task<(int quantity, decimal cartTotal)> ChangeCartQuantityAsync(int touristId, int tripId, int delta)
        {
            var item = await _context.TouristCarts.FirstOrDefaultAsync(c => c.TouristId == touristId && c.TripId == tripId);
            if (item == null) return (0, await GetCartTotalAsync(touristId));

            item.Quantity += delta;
            if (item.Quantity <= 0) _context.TouristCarts.Remove(item);
            else { item.TotalPrice = item.UnitPrice * item.Quantity; _context.TouristCarts.Update(item); }

            await _context.SaveChangesAsync();
            return (item.Quantity > 0 ? item.Quantity : 0, await GetCartTotalAsync(touristId));
        }

        public async Task ClearCartAsync(int touristId) { var items = _context.TouristCarts.Where(c => c.TouristId == touristId); _context.TouristCarts.RemoveRange(items); await _context.SaveChangesAsync(); }
        public async Task RemoveCartItemAsync(TouristCart cartItem) { _context.TouristCarts.Remove(cartItem); await _context.SaveChangesAsync(); }
        public async Task<TouristCart?> GetCartItemByIdAsync(int cartItemId) => await _context.TouristCarts.Include(c => c.Trip).Include(c => c.Tourist).FirstOrDefaultAsync(c => c.Id == cartItemId);
        public async Task<IEnumerable<TouristCart>> GetCartItemsWithTripAsync(string userId) => await _context.TouristCarts.Where(c => c.UserId == userId).Include(c => c.Trip).ToListAsync();
        public async Task RemoveCartItemsAsync(IEnumerable<TouristCart> items) { _context.TouristCarts.RemoveRange(items); await _context.SaveChangesAsync(); }

        // -------------------- CREDIT CARD --------------------
        public async Task<CreditCard?> GetCreditCardAsync(string number, string cvv, string holder, string expiry)
        {
            if (string.IsNullOrWhiteSpace(number) ||
                string.IsNullOrWhiteSpace(cvv) ||
                string.IsNullOrWhiteSpace(holder) ||
                string.IsNullOrWhiteSpace(expiry))
            {
                return null;
            }

            var num = number.Trim();
            var code = cvv.Trim();
            var holderNormalized = holder.Trim().ToLower();
            var exp = expiry.Trim();

            return await _context.CreditCards
                .FirstOrDefaultAsync(c =>
                    c.CardNumber == num
                    && c.CVV == code
                    && c.ExpiryDate == exp
                    && c.CardHolder != null
                    && c.CardHolder.ToLower() == holderNormalized
                );
        }

        public async Task UpdateCreditCardAsync(CreditCard card) { _context.CreditCards.Update(card); await _context.SaveChangesAsync(); }

        // -------------------- BOOKING & SAVE --------------------
        public async Task AddBookingAsync(PaymentTripBooking booking) => await _context.paymentTripBookings.AddAsync(booking);
        public async Task SaveAsync() => await _context.SaveChangesAsync();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        // -------------------- TRANSACTIONS --------------------
        public bool SupportsTransactions => true;
        public async Task BeginTransactionAsync() { _transaction = await _context.Database.BeginTransactionAsync(); }
        public async Task CommitTransactionAsync() { if (_transaction != null) { await _transaction.CommitAsync(); await _transaction.DisposeAsync(); _transaction = null; } }
        public async Task RollbackTransactionAsync() { if (_transaction != null) { await _transaction.RollbackAsync(); await _transaction.DisposeAsync(); _transaction = null; } }

        // -------------------- LOOKUPS --------------------
        public async Task<Tourist?> GetTouristByEmailAsync(string email) => await _context.Tourists.FirstOrDefaultAsync(t => t.email == email);
        public async Task<Trip?> GetTripByIdAsync(int id) => await _context.Trips.FirstOrDefaultAsync(t => t.id == id);

        // ✅ NEW IMPLEMENTATION
        public async Task CreditTourGuideAsync(int tourGuideId, decimal amount)
        {
            // 1. Find the Tour Guide
            var guide = await _context.TourGuides.FindAsync(tourGuideId);
            if (guide == null) return;

            // 2. Find the Guide's Credit Card
            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.UserId == tourGuideId || c.CardHolder.ToLower() == (guide.firstName + " " + guide.lastName).ToLower());

            if (card != null)
            {
                // Transfer money to the card
                card.Balance += amount;
                _context.CreditCards.Update(card);
            }
        }
    }
}