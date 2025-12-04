using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;

namespace Tourism.Repository
{

    public class MerchantRepository:IMerchantRepository
    {
        private readonly TourismDbContext _context;

        public MerchantRepository(TourismDbContext context)
        {
            _context = context;
        }

        public Merchant GetByEmail(string email)
        {
            return _context.Merchants.FirstOrDefault(m => m.email == email);
        }

        public void UpdateVerificationDocument(Merchant m, byte[] pdfBytes)
        {
            m.verificationDocuments = pdfBytes;
            _context.Update(m);
            _context.SaveChanges();

        }

        public List<string> GetMessages(int id)
        {
            return _context.InboxMsgs
                .Where(i => i.providerId == id && i.providerType == "Merchant")
                .OrderByDescending(i => i.date)
                .Select(i => i.content)
                .Take(10)
                .ToList();
        }

        public CreditCard GetCC(CreditCard cc)
        {
            return _context.CreditCards.FirstOrDefault(c =>
         c.CardNumber == cc.CardNumber &&
         c.CVV == cc.CVV &&
         c.ExpiryDate == cc.ExpiryDate && c.CardHolder == cc.CardHolder);

        }
        public ServiceRequest GetServiceRequest(int productId)
        {
            return _context.ServiceRequests.FirstOrDefault(s => s.serviceId == productId && s.role == "Merchant");
        }
    }
}
