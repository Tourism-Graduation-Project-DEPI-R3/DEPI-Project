using Tourism.Models.Relations;
using Tourism.Models;
using Microsoft.EntityFrameworkCore;

namespace Tourism.IRepository
{
    public interface IUnitOfWork
    {
        IRepository<Tourist> Tourists { get; }
        IRepository<Merchant> Merchants { get; }
        IRepository<Hotel> Hotels { get; }
        IRepository<TourGuide> TourGuides { get; }
        IRepository<Restaurant> Restaurants { get; }
        IRepository<Product> Products { get; }
        IRepository<Trip> Trips { get; }
        IRepository<Meal> Meals { get; }
        IRepository<Room> Rooms { get; }
        IRepository<ServiceRequest> ServiceRequests { get; }
        IRepository<VerificationRequest> VerificationRequests { get; }
        IRepository<InboxMsg> InboxMessages { get; }
        IRepository<Image> Images { get; }
        IRepository<CartProduct> Cart{get;}
        IRepository<FavouriteProduct>? Favourites { get; }
      
        Task<int> SaveAsync();

        public CreditCard GetCC(CreditCard cc);


    }
}
