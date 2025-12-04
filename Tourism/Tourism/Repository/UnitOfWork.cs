using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.Repository;

namespace Tourism.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly TourismDbContext _context;

        // Private fields for repositories
        private IRepository<Tourist>? _tourists;
        private IRepository<Merchant>? _merchants;
        private IRepository<Hotel>? _hotels;
        private IRepository<TourGuide>? _tourGuides;
        private IRepository<Restaurant>? _restaurants;
        private IRepository<Product>? _products;
        private IRepository<Trip>? _trips;
        private IRepository<Meal>? _meals;
        private IRepository<Table>? _tables;
        private IRepository<Room>? _rooms;
        private IRepository<ServiceRequest>? _serviceRequests;
        private IRepository<VerificationRequest>? _verificationRequests;
        private IRepository<InboxMsg>? _inboxMessages;
        private IRepository<Image>? _images;
        private IRepository<CartProduct>? _cart;
        private IRepository<FavouriteProduct>? _favourites;
        private IRepository<TouristRestaurant>? _touristRestaurants;
        private IRepository<CreditCard>? _creditCards;
        private IRepository<TouristRoom> _touristRooms;


        public UnitOfWork(TourismDbContext context)
        {
            _context = context;
        }

        public IRepository<Tourist> Tourists => _tourists ??= new Repository<Tourist>(_context);
        public IRepository<Merchant> Merchants => _merchants ??= new Repository<Merchant>(_context);
        public IRepository<Hotel> Hotels => _hotels ??= new Repository<Hotel>(_context);
        public IRepository<TourGuide> TourGuides => _tourGuides ??= new Repository<TourGuide>(_context);
        public IRepository<Restaurant> Restaurants => _restaurants ??= new Repository<Restaurant>(_context);
        public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
        public IRepository<Trip> Trips => _trips ??= new Repository<Trip>(_context);
        public IRepository<Meal> Meals => _meals ??= new Repository<Meal>(_context);
        public IRepository<Room> Rooms => _rooms ??= new Repository<Room>(_context);
        public IRepository<Table> Tables => _tables ??= new Repository<Table>(_context);
        public IRepository<ServiceRequest> ServiceRequests => _serviceRequests ??= new Repository<ServiceRequest>(_context);
        public IRepository<VerificationRequest> VerificationRequests => _verificationRequests ??= new Repository<VerificationRequest>(_context);
        public IRepository<InboxMsg> InboxMessages => _inboxMessages ??= new Repository<InboxMsg>(_context);
        public IRepository<Image> Images => _images ??= new Repository<Image>(_context);
        public IRepository<CartProduct> Cart => _cart ??= new Repository<CartProduct>(_context);
        public IRepository<FavouriteProduct> Favourites => _favourites ?? (_favourites = new Repository<FavouriteProduct>(_context));
        public IRepository<TouristRestaurant> TouristRestaurants => _touristRestaurants ??= new Repository<TouristRestaurant>(_context);
        public IRepository<CreditCard> CreditCards => _creditCards ??= new Repository<CreditCard>(_context);

        public IRepository<TouristRoom> TouristRooms => _touristRooms ?? (_touristRooms = new Repository<TouristRoom>(_context));



        // Save all changes
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Dispose pattern
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public CreditCard GetCC(CreditCard cc)
        {
            // Ensure all fields are trimmed and handle null values
            var cardNumber = cc.CardNumber?.Trim();
            var cvv = cc.CVV?.Trim();
            var expiryDate = cc.ExpiryDate?.Trim();
            var cardHolder = cc.CardHolder?.Trim();

            if (string.IsNullOrWhiteSpace(cardNumber) || 
                string.IsNullOrWhiteSpace(cvv) || 
                string.IsNullOrWhiteSpace(expiryDate) || 
                string.IsNullOrWhiteSpace(cardHolder))
            {
                return null;
            }

            return _context.CreditCards.FirstOrDefault(c =>
                c.CardNumber.Trim() == cardNumber &&
                c.CVV.Trim() == cvv &&
                c.ExpiryDate.Trim() == expiryDate &&
                c.CardHolder.Trim().ToLower() == cardHolder.ToLower());
        }

        public async Task<CreditCard> GetCCByCardNumberAsync(string cardNumber)
        {
            return await _context.CreditCards
                .FirstOrDefaultAsync(cc => cc.CardNumber == cardNumber);
        }
    }
}
