using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class TourGuideRepository : ITourGuideRepository
    {
        private readonly TourismDbContext _context;

        public TourGuideRepository(TourismDbContext context)
        {
            _context = context;
        }

        public async Task<TourGuide?> GetByIdAsync(int id)
        {
            return await _context.TourGuides
                .Include(g => g.creditCard)
                .FirstOrDefaultAsync(g => g.TourGuideId == id);
        }

        public async Task<TourGuide?> GetByEmailAsync(string email)
        {
            return await _context.TourGuides
                .Include(g => g.creditCard)
                .FirstOrDefaultAsync(g => g.email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.TourGuides.AnyAsync(g => g.email == email);
        }

        public async Task AddAsync(TourGuide guide)
        {
            await _context.TourGuides.AddAsync(guide);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<CreditCard?> GetCreditCardAsync(string number, string cvv, string holder, string expiry)
        {
            if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(cvv) ||
                string.IsNullOrWhiteSpace(holder) || string.IsNullOrWhiteSpace(expiry)) return null;

            var holderNormalized = holder.Trim().ToLower();
            return await _context.CreditCards
                .FirstOrDefaultAsync(c =>
                    c.CardNumber == number.Trim()
                    && c.CVV == cvv.Trim()
                    && c.ExpiryDate == expiry.Trim()
                    && c.CardHolder != null
                    && c.CardHolder.ToLower() == holderNormalized
                );
        }


        public async Task LinkCreditCardAsync(int tourGuideId, string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return;

            // find card by card number
            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
            if (card == null) return; // nothing to link

            // If card already linked to same guide -> nothing to do
            if (card.UserId.HasValue && card.UserId.Value == tourGuideId)
                return;

            // If card already linked to another user -> skip (or throw depending on policy)
            if (card.UserId.HasValue && card.UserId.Value != tourGuideId)
            {
                // Option A: skip silently (current behavior)
                return;

                // Option B: throw to notify caller
                // throw new InvalidOperationException("Card already linked to another user.");
            }

            // Link the card (UserId is int?)
            card.UserId = tourGuideId;

            // persist change
            _context.CreditCards.Update(card);
            await _context.SaveChangesAsync();
        }


        public async Task<CreditCard> FindFirstUnlinkedCardByHolderAsync(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return null;

            var lowered = fullName.ToLower();
            return await _context.CreditCards
                .FirstOrDefaultAsync(c => c.CardHolder != null
                                           && c.CardHolder.ToLower() == lowered
                                           && !c.UserId.HasValue);
        }

        public async Task<IList<CreditCard>> GetCreditCardsAsync(int tourGuideId)
        {
            return await _context.CreditCards
                .Where(c => c.UserId.HasValue && c.UserId.Value == tourGuideId)
                .ToListAsync();
        }



    }
}
