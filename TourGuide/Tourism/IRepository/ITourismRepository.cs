using Tourism.Models;

namespace Tourism.IRepository
{
    public interface ITourismRepository
    {
        // Tourist
        Task<bool> TouristEmailExistsAsync(string email);
        Task AddTouristAsync(Tourist tourist);

        // Hotel
        Task<bool> HotelEmailExistsAsync(string email);
        Task AddHotelAsync(Hotel hotel);

        // Login
        Task<Tourist?> GetTouristByEmailAsync(string email);

        // Save changes
        Task SaveChangesAsync();


    }
}
