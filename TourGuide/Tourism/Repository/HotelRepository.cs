using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class HotelRepository: IHotelRepository
    {
        private readonly TourismDbContext _context;

        public HotelRepository(TourismDbContext context)
        {
            _context = context;
        }

     

    }
}
