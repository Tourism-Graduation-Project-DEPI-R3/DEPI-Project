using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tourism.Models;

namespace Tourism.IRepository
{
    public interface ITripRepository
    {
        // Basic retrievals
        Task<List<Trip>> GetAllTripsAsync();
        Task<Trip?> GetTripByIdAsync(int id);              // includes related TourPlans
        Task<Trip?> GetByIdAsync(int id);                  // no includes, lightweight

        // Retrievals with different needs
        Task<Trip?> GetByIdWithDetailsAsync(int id);       // include TourGuide, TourPlans, etc.
        Task<List<Trip>> GetAllAsync();
        Task<IEnumerable<Trip>> GetTripsByTourGuideIdAsync(int tourGuideId);
        Task<List<Trip>> GetByTourGuideIdAsync(int tourGuideId);
        Task<List<Trip>> GetAcceptedActiveTripsAsync();
        Task<List<Trip>> GetPendingTripsAsync();
        Task<List<Trip>> GetTripsWithBookingsByGuideIdAsync(int tourGuideId);

        // Count
        Task<int> GetTripsCountAsync();

        // CRUD operations
        Task AddAsync(Trip trip);
        Task DeleteAsync(Trip trip);
        Task UpdateAsync(Trip trip);
        void Update(Trip trip);
        Task UpdateTripAsync(Trip trip);
        void Remove(Trip trip);

        // Bulk ops / predicate-based
        Task<IEnumerable<TouristCart>> GetWhereAsync(Expression<Func<TouristCart, bool>> predicate);
        Task DeleteRangeAsync(IEnumerable<TouristCart> carts);

        // Other helpers
        Task<TourGuide?> GetByEmailAsync(string email);

        // Save
        Task SaveChangesAsync();
        Task SaveAsync();
    }
}
