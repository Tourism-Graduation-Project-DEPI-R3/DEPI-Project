using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class RestaurantRepository: IRestaurantRepository
    {
        private readonly TourismDbContext _context;

        public RestaurantRepository(TourismDbContext context)
        {
            _context = context;
        }


    }
}
