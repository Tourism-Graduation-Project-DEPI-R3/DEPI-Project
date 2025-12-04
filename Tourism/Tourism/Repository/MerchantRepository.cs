using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

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
        public List<ProductOrdersViewModel> GetOrders(int merchantId)
        {
            List<ProductOrdersViewModel> orders = (
              from tp in _context.TouristProducts
              join t in _context.Tourists on tp.touristId equals t.id
              join p in _context.Products on tp.productId equals p.id
              where p.merchantId == merchantId   
              select new ProductOrdersViewModel
      {
          productName = p.name,
          orderDate = tp.orderDate,
          amount = tp.amount,
          status = tp.status,
          TouristName = t.firstName,
          address = tp.address
      })
      .ToList();

            return orders;
        }

        public decimal MonthlyEarningsProducts(int month, int merchantId)
        {
            var today = DateTime.Today;
            int currentYear = today.Year;
            var monthStart = new DateTime(currentYear, month, 1);
            var monthEnd = month == 12
                ? new DateTime(currentYear + 1, 1, 1)
                : new DateTime(currentYear, month + 1, 1);

            decimal monthlyEarnings = _context.TouristProducts
                .Where(tp => (tp.status == "Delivered" || tp.status == "Processing")
                             && tp.orderDate >= monthStart
                             && tp.orderDate < monthEnd)
                .Join(_context.Products,
                      tp => tp.productId,
                      p => p.id,
                      (tp, p) => new { tp, p })
                .Where(joined => joined.p.merchantId == merchantId)
                .Sum(joined => (decimal?)joined.tp.price) ?? 0;

            return monthlyEarnings;
        }

        public int GetSumAmountProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return _context.TouristProducts
                .Where(tp => (tp.status == "Delivered" || tp.status == "Processing")
                             && tp.orderDate >= startDate
                             && tp.orderDate < today.AddDays(1))
                .Join(_context.Products,
                      tp => tp.productId,
                      p => p.id,
                      (tp, p) => new { tp, p })
                .Where(joined => joined.p.merchantId == merchantId)
                .Sum(joined => joined.tp.amount);
        }

        public double GetSumPriceProducts(int days, int merchantId)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-days);
            return (double) _context.TouristProducts
                .Where(tp => (tp.status == "Delivered" || tp.status == "Processing")
                             && tp.orderDate >= startDate
                             && tp.orderDate < today.AddDays(1))
                .Join(_context.Products,
                      tp => tp.productId,
                      p => p.id,
                      (tp, p) => new { tp, p })
                .Where(joined => joined.p.merchantId == merchantId)
                .Sum(joined => joined.tp.price);
        }

        public List<Review> GetReviews(int merchantId, int stars)
        {
            var reviews =
                (from tf in _context.TouristFeedbacks
                 join p in _context.Products
                     on tf.touristId equals p.id   
                 where tf.targetType == "Product"
                       && tf.rating == stars
                       && p.merchantId == merchantId
                 select new Review
                 {
                     Comment = tf.comment,
                     Rating = tf.rating,
                     CreatedAt = tf.createdAt
                 }).ToList();

            return reviews;
        }


        public void PrepareDashboard(MerchantDashboardViewModel dashboardModel, int id)
        {
            var merchant = _context.Merchants.Find(id);
            var today = DateTime.Today;
            dashboardModel.name = merchant.name;

            dashboardModel.units = new List<int>(new int[4]);
            dashboardModel.total = new List<double>(new double[4]);
            dashboardModel.reviews = Enumerable
           .Range(0, 6)
           .Select(_ => new List<Review>())
           .ToList();
            dashboardModel.annualEarnings = new List<double>(new double[12]);

            int currentYear = today.Year;

            for (int month = 1; month <= 12; month++)
            {
                dashboardModel.annualEarnings[month - 1] = (double)MonthlyEarningsProducts(month, id);
            }
            dashboardModel.units[0] = GetSumAmountProducts(0, id);

            dashboardModel.units[1] = GetSumAmountProducts(7, id);
            dashboardModel.units[2] =GetSumAmountProducts(14, id);
            dashboardModel.units[3] = GetSumAmountProducts(30, id);

            dashboardModel.total[0] = GetSumPriceProducts(0, id);
            dashboardModel.total[1] = GetSumPriceProducts(7, id);
            dashboardModel.total[2] = GetSumPriceProducts(14, id);
            dashboardModel.total[3] = GetSumPriceProducts(30, id);

            dashboardModel.msg = GetMessages(id);

            for (int i = 0; i <= 5; i++) {
                dashboardModel.reviews[i] = GetReviews(id, i);
            }

            var ratings = (from tf in _context.TouristFeedbacks
                          join p in _context.Products
                             on tf.targetId equals p.id
                          where tf.targetType == "Product"
                                && p.merchantId == id
                          select tf.rating);
            
            dashboardModel.rate = ratings.Any() ? (int)ratings.Average() : 0;

        }

    }
}
