using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.Repository
{
    public class TouristRepository:ITouristRepository
    {
        private readonly TourismDbContext _context;
        private IDbContextTransaction? _transaction;

        public TouristRepository(TourismDbContext context)
        {
            _context = context;
        }
        public Tourist GetByEmail(string email)
        {
            return _context.Tourists.FirstOrDefault(t => t.email == email);
        }
        public double TotalEearningsRestaurants()
        {
            return ((
                from tb in _context.Tables
                from tr in _context.TouristRestaurants
                where tb.restaurantId == tr.restaurantId && tb.number == tr.tableNumber
                select (double?)tb.bookingPrice
            ).Sum() ?? 0);
        }

        public double TotalEearningsProducts()
        {
            return ((
                from tp in _context.TouristProducts
                join p in _context.Products on tp.productId equals p.id
                where tp.status == "Delivered" || tp.status=="Processing"
                select (double?)p.price
            ).Sum() ?? 0);
        }

        public double TotalEearningsHotels()
        {
            return ((
                from r in _context.Rooms
                join tr in _context.TouristRooms on r.id equals tr.roomId
                select (double?)r.cost
            ).Sum() ?? 0);
        }
        public double TotalEearningsTrips()
        {
            return ((
                from tb in _context.Tables
                from tr in _context.TouristRestaurants
                where tb.restaurantId == tr.restaurantId && tb.number == tr.tableNumber
                select (double?)tb.bookingPrice
            ).Sum() ?? 0);
        }
      
        public bool AddToCart(int productId,int touristId)
        {
            CartProduct c = new();
            c.ProductId = productId;
            c.Product =  _context.Products.Find(productId);
            if (c.Product == null)
                return false;
            
            c.TouristId = touristId;
            c.Tourist = _context.Tourists.Find(touristId);
            if (c.Tourist == null)
                return false;

            var existingCartItem = _context.CartProducts
       .FirstOrDefault(c => c.ProductId == productId && c.TouristId == touristId);
            if (existingCartItem == null)
            {
                _context.CartProducts.Add(c);
                _context.SaveChanges();
            }
            return true;
        }
        public void transaction(CreditCard cardfromrequest, decimal money, string operation, int id)
        {
            var mainCC = _context.CreditCards.Find("00000000000000");
            
            // Create main system credit card if it doesn't exist
            if (mainCC == null)
            {
                mainCC = new CreditCard
                {
                    CardNumber = "00000000000000",
                    Balance = 0,
                    CardHolder = "System",
                    ExpiryDate = "12/99",
                    CVV = "000",
                    UserId = null
                };
                _context.CreditCards.Add(mainCC);
                _context.SaveChanges();
            }
            
            var tourist = _context.Tourists.FirstOrDefault(t => t.id == id);
            if (operation == "withdraw")
            {
                cardfromrequest.Balance += money;
                tourist.balance -= (double)money;
                mainCC.Balance -= money;
            }
            else
            {
                cardfromrequest.Balance -= money;
                tourist.balance += (double)money;
                mainCC.Balance += money;
            }
            _context.SaveChanges();
        }

        public void BuyProduct(int touristId,bool buyCredit,CreditCard? cc,ProductViewModel product,string location)
        {
            var tourist = _context.Tourists.Find(touristId);
            if (buyCredit==true) cc.Balance -= (decimal) product.price*product.count;
            else
            {
                tourist.balance -= product.price*product.count;
            }
            var creditcard = _context.CreditCards.Find("00000000000000");
            creditcard.Balance += (decimal) product.price * product.count;

            TouristProduct tp = new();
            tp.product = _context.Products.Find(product.productId);
            tp.tourist = tourist;
            tp.productId = (int)product.productId;
            tp.touristId = touristId;
            tp.amount = product.count;
            tp.status = "Processing";
            tp.address = location;
            tp.funded = false;
            tp.price = (Decimal) product.price;
            _context.TouristProducts.Add(tp);

            var p = _context.Products.Find(product.productId);
            p.count--;

            var msg = new InboxMsg
            {
                providerType = "Merchant",
                content = $"{tourist.firstName} has ordered {product.count} unit(s) of {product.name}.",
                providerId = (int)product.id,
                date = DateTime.Now
            };
            _context.InboxMsgs.Add(msg);

            var cp = _context.CartProducts.FirstOrDefault(c => c.ProductId == product.productId && c.TouristId == touristId);
            if (cp != null) _context.CartProducts.Remove(cp);
            if(p!=null) p.count--;
            _context.SaveChanges();
        }

        public void RemoveFromCart(int productId,int touristId)
        {
            var record = _context.CartProducts.FirstOrDefault(cp => cp.ProductId == productId && cp.TouristId == touristId);
            if (record != null)
            {
                _context.CartProducts.Remove(record);
                _context.SaveChanges();
            }
        }

        public void CancelOrder(int orderId)
        {
            var order = _context.TouristProducts.Find(orderId);
            order.status = "Cancelled";
            var cc = _context.CreditCards.Find("00000000000000");
            var product = _context.Products.Find(order.productId);
            var merchant = _context.Merchants.Find(product.merchantId);
            var tourist = _context.Tourists.Find(order.touristId);
            decimal total = (decimal) product.price * (decimal) (1 - product.offer / 100) * order.amount;
            cc.Balance -= total;
            tourist.balance += (double) total;
            order.status = "Cancelled";
            var msg = new InboxMsg
            {
                providerType = "Merchant",
                content = $"The Order (Product: {product.name}, Amount: {order.amount}, Customer: {tourist.firstName}) has been cancelled.",
                providerId =merchant.id,
                date = DateTime.Now
            };
            _context.InboxMsgs.Add(msg);
            _context.SaveChanges();
        }

        public List<ProductOrdersViewModel> GetOrders(int touristId)
        {
            List<ProductOrdersViewModel> orders = (
              from tp in _context.TouristProducts
              join t in _context.Tourists on tp.touristId equals t.id
              join p in _context.Products on tp.productId equals p.id
              where tp.touristId == touristId
              select new ProductOrdersViewModel
              {
                  orderId = tp.orderId,
                  productName = p.name,
                  orderDate = tp.orderDate,
                  arrivalDate=tp.arrivalDate,
                  amount = tp.amount,
                  status = tp.status,
                  address = tp.address,
                  refund = (tp.arrivalDate==null ||((DateTime.Today - tp.arrivalDate.Value).Days <= 14))? true:false
              })
      .ToList();

            return orders;
        }


        public bool review(int touristId,int serviceId,int rate,string comment,string type)
        {

            if (type == "Product")
            {
                var check = _context.TouristProducts.FirstOrDefault(tf => tf.touristId == touristId && tf.productId == serviceId && tf.status== "Delivered");
                if (check == null) return false;
            }
            var record = _context.TouristFeedbacks.FirstOrDefault(f => f.touristId == touristId && f.targetType == type && f.targetId == serviceId);
            if (record != null) _context.TouristFeedbacks.Remove(record);

            TouristFeedback feedback = new();
            feedback.touristId = touristId;
            feedback.targetId = serviceId;
            feedback.rating = rate;
            feedback.targetType = type;
            feedback.createdAt = DateTime.Today;
            feedback.comment = comment;
            _context.TouristFeedbacks.Add(feedback);
            _context.SaveChanges();
            return true;
        }

        // ========== NEW ASYNC METHODS ==========
        public decimal MonthlyEarningsProducts(int month, int merchantId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, month, 1);
            var monthEnd = month == 12 ? new DateTime(today.Year + 1, 1, 1) : new DateTime(today.Year, month + 1, 1);
            return _context.TouristProducts.Where(tp => tp.status == "Delivered" && tp.orderDate >= monthStart && tp.orderDate < monthEnd)
                .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId)
                .Sum(x => (decimal?)x.p.price * x.tp.amount) ?? 0;
        }

        public int GetSumAmountProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return _context.TouristProducts.Where(tp => tp.status == "Delivered" && tp.orderDate >= startDate && tp.orderDate < today.AddDays(1))
                .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId).Sum(x => x.tp.amount);
        }

        public double GetSumPriceProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return _context.TouristProducts.Where(tp => tp.status == "Delivered" && tp.orderDate >= startDate && tp.orderDate < today.AddDays(1))
                 .Join(_context.Products, tp => tp.productId, p => p.id, (tp, p) => new { tp, p }).Where(x => x.p.merchantId == merchantId).Sum(x => (double?)x.p.price * x.tp.amount) ?? 0;
        }

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
            var mainCC = await _context.CreditCards.FindAsync("00000000000000");
            var tourist = await _context.Tourists.FirstOrDefaultAsync(t => t.id == id);
            if (tourist == null) return;

            if (operation == "withdraw") { cardFromRequest.Balance += money; tourist.balance -= (double)money; if (mainCC != null) mainCC.Balance -= money; }
            else { cardFromRequest.Balance -= money; tourist.balance += (double)money; if (mainCC != null) mainCC.Balance += money; }

            _context.CreditCards.Update(cardFromRequest);
            _context.Tourists.Update(tourist);
            await _context.SaveChangesAsync();
        }

        public async Task BuyProductAsync(int touristId, bool buyCredit, CreditCard? cc, ProductViewModel product)
        {
            var tourist = await _context.Tourists.FindAsync(touristId);
            if (tourist == null) return;
            decimal totalPrice = (decimal)(product.price * product.count);

            if (buyCredit) { if (cc != null) { cc.Balance -= totalPrice; _context.CreditCards.Update(cc); } }
            else { tourist.balance -= (double)totalPrice; _context.Tourists.Update(tourist); }

            var prod = await _context.Products.FindAsync(product.productId);
            var tp = new TouristProduct { product = prod, tourist = tourist, productId = (int)product.productId, touristId = touristId, amount = product.count, status = "Processing", arrivalDate = DateTime.Today.AddDays((int)product.DeliversWithin), price = (decimal)product.price };
            await _context.TouristProducts.AddAsync(tp);

            await _context.InboxMsgs.AddAsync(new InboxMsg { providerType = "Merchant", content = $"{tourist.firstName} has ordered {product.count} unit(s) of {product.name}.", providerId = (int)product.id, date = DateTime.Today });

            var cp = await _context.CartProducts.FirstOrDefaultAsync(c => c.ProductId == product.productId && c.TouristId == touristId);
            if (cp != null) _context.CartProducts.Remove(cp);
            if (prod != null) prod.count--;
            await _context.SaveChangesAsync();
        }

        // ========== TRIP CART OPERATIONS ==========
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

        // ========== CREDIT CARD ==========
        public async Task<CreditCard?> GetCreditCardAsync(string number, string cvv, string holder, string expiry)
        {
            if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(cvv) || string.IsNullOrWhiteSpace(holder) || string.IsNullOrWhiteSpace(expiry)) return null;

            var num = number.Trim();
            var code = cvv.Trim();
            var holderNormalized = holder.Trim().ToLower();
            var exp = expiry.Trim();

            return await _context.CreditCards.FirstOrDefaultAsync(c => c.CardNumber == num && c.CVV == code && c.ExpiryDate == exp && c.CardHolder != null && c.CardHolder.ToLower() == holderNormalized);
        }

        public async Task UpdateCreditCardAsync(CreditCard card) { _context.CreditCards.Update(card); await _context.SaveChangesAsync(); }

        // ========== BOOKING & SAVE ==========
        public async Task AddBookingAsync(PaymentTripBooking booking) => await _context.PaymentTripBookings.AddAsync(booking);
        public async Task SaveAsync() => await _context.SaveChangesAsync();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        // ========== TRANSACTIONS ==========
        public bool SupportsTransactions => true;
        public async Task BeginTransactionAsync() { _transaction = await _context.Database.BeginTransactionAsync(); }
        public async Task CommitTransactionAsync() { if (_transaction != null) { await _transaction.CommitAsync(); await _transaction.DisposeAsync(); _transaction = null; } }
        public async Task RollbackTransactionAsync() { if (_transaction != null) { await _transaction.RollbackAsync(); await _transaction.DisposeAsync(); _transaction = null; } }

        // ========== LOOKUPS ==========
        public async Task<Tourist?> GetTouristByEmailAsync(string email) => await _context.Tourists.FirstOrDefaultAsync(t => t.email == email);
        public async Task<Trip?> GetTripByIdAsync(int id) => await _context.Trips.FirstOrDefaultAsync(t => t.id == id);

        // ========== TOUR GUIDE PAYMENT ==========
        public async Task CreditTourGuideAsync(int tourGuideId, decimal amount)
        {
            var guide = await _context.TourGuides.FindAsync(tourGuideId);
            if (guide == null) return;

            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.UserId == tourGuideId || (c.CardHolder != null && c.CardHolder.ToLower() == (guide.firstName + " " + guide.lastName).ToLower()));

            if (card != null)
            {
                card.Balance += amount;
                _context.CreditCards.Update(card);
                await _context.SaveChangesAsync();
            }
        }
    }
}
