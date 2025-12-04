using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class TourismRepository : ITourismRepository
    {
        private readonly TourismDbContext _context;

        public TourismRepository(TourismDbContext context)
        {
            _context = context;
        }

        // ===== Tourist methods =====
        public async Task<bool> TouristEmailExistsAsync(string email)
        {
            return await _context.Tourists.AnyAsync(t => t.email == email);
        }

        public async Task AddTouristAsync(Tourist tourist)
        {
            await _context.Tourists.AddAsync(tourist);
        }

        // ===== Hotel methods =====
        public async Task<bool> HotelEmailExistsAsync(string email)
        {
            return await _context.Hotels.AnyAsync(h => h.email == email);
        }

        public async Task AddHotelAsync(Hotel hotel)
        {
            await _context.Hotels.AddAsync(hotel);
        }

        // ===== Login / Tourist =====
        public async Task<Tourist?> GetTouristByEmailAsync(string email)
        {
            return await _context.Tourists.FirstOrDefaultAsync(t => t.email == email);
        }

        // ===== Save =====
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
