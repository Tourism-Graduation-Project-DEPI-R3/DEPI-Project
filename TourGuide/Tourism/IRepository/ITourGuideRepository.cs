using Tourism.Models;

namespace Tourism.IRepository
{
    public interface ITourGuideRepository
    {
        Task<TourGuide?> GetByIdAsync(int id);
        Task<TourGuide?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(TourGuide guide);
        Task SaveChangesAsync();

        Task<IList<CreditCard>> GetCreditCardsAsync(int tourGuideId);
        Task LinkCreditCardAsync(int tourGuideId, string cardNumber);
        Task<CreditCard> FindFirstUnlinkedCardByHolderAsync(string fullName);





    }
}
