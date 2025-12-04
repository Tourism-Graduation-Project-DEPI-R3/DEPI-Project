using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class TripRepository : ITripRepository
    {
        private readonly TourismDbContext _context;

        public TripRepository(TourismDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Trip>> GetAllTripsAsync() => await _context.Trips.ToListAsync();
        public async Task<Trip?> GetTripByIdAsync(int id) => await _context.Trips.Include(t => t.TourPlans).FirstOrDefaultAsync(t => t.id == id);
        public async Task<Trip?> GetByIdAsync(int id) => await _context.Trips.FirstOrDefaultAsync(t => t.id == id);
        public async Task<Trip?> GetByIdWithDetailsAsync(int id) => await _context.Trips.Include(t => t.TourGuide).Include(t => t.TourPlans).FirstOrDefaultAsync(t => t.id == id);
        public async Task<List<Trip>> GetAllAsync() => await _context.Trips.ToListAsync();

        public async Task<IEnumerable<Trip>> GetTripsByTourGuideIdAsync(int tourGuideId) => await _context.Trips.Where(t => t.tourGuideId == tourGuideId).ToListAsync();
        public async Task<List<Trip>> GetByTourGuideIdAsync(int tourGuideId) => await _context.Trips.Where(t => t.tourGuideId == tourGuideId).ToListAsync();
        public async Task<List<Trip>> GetAcceptedActiveTripsAsync() => await _context.Trips.Where(t => t.accepted && t.status).ToListAsync();
        public async Task<List<Trip>> GetPendingTripsAsync() => await _context.Trips.Where(t => !t.accepted).ToListAsync();
        public async Task<List<Trip>> GetTripsWithBookingsByGuideIdAsync(int tourGuideId) => await _context.Trips.Where(t => t.tourGuideId == tourGuideId).Include(t => t.TouristCarts).ThenInclude(tc => tc.Tourist).ToListAsync();
        public async Task<int> GetTripsCountAsync() => await _context.Trips.CountAsync();

        // ----------------- Modifiers -----------------
        public async Task AddAsync(Trip trip) => await _context.Trips.AddAsync(trip);
        public async Task DeleteAsync(Trip trip) { _context.Trips.Remove(trip); await Task.CompletedTask; }
        public void Remove(Trip trip) => _context.Trips.Remove(trip);

        public async Task UpdateAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            await Task.CompletedTask;
        }

        public void Update(Trip trip) => _context.Trips.Update(trip);
        public async Task UpdateTripAsync(Trip trip) { _context.Trips.Update(trip); await Task.CompletedTask; }

        // ----------------- Bulk / predicate -----------------
        public async Task<IEnumerable<TouristCart>> GetWhereAsync(Expression<Func<TouristCart, bool>> predicate) => await _context.TouristCarts.Where(predicate).ToListAsync();
        public async Task DeleteRangeAsync(IEnumerable<TouristCart> carts) { _context.TouristCarts.RemoveRange(carts); await Task.CompletedTask; }
        public async Task<TourGuide?> GetByEmailAsync(string email) => await _context.TourGuides.FirstOrDefaultAsync(t => t.email == email);

        // ----------------- Save -----------------
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
