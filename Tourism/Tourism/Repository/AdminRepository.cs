using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly TourismDbContext _context;

        public AdminRepository(TourismDbContext context)
        {
            _context = context;
        }
        public Admin GetByEmail(string email)
        {
            return _context.Admins.FirstOrDefault(a => a.email == email);
        }

        public List<VerificationViewModel> GetVerificationViewModels(string type)
        {
            List<VerificationViewModel> requests = new();

            if (type == "Merchant")
            {
                requests = _context.Merchants
                    .Join(
                        _context.VerificationRequests,
                        m => m.id,
                        v => v.provider_Id,
                        (m, v) => new { Merchant = m, Request = v }
                    )
                    .Where(joined => joined.Request.role == "Merchant")
                    .Select(joined => new VerificationViewModel
                    {
                        providerId = joined.Merchant.id,
                        name = joined.Merchant.name,
                        email = joined.Merchant.email,
                        pdf = joined.Merchant.verificationDocuments,
                        type = "Merchant",
                        id = joined.Request.id
                    })
                    .ToList();
            }
            else if (type == "TourGuide")
            {
                requests = _context.TourGuides
                    .Join(_context.VerificationRequests,
                          t => t.TourGuideId,
                          v => v.provider_Id,
                          (t, v) => new { TourGuide = t, Request = v })
                    .Where(joined => joined.Request.role == "TourGuide")
                    .Select(joined => new VerificationViewModel
                    {
                        providerId = joined.TourGuide.TourGuideId,
                        name = joined.TourGuide.firstName,
                        email = joined.TourGuide.email,
                        pdf = joined.TourGuide.verificationDocuments,
                        type = "TourGuide",
                        id = joined.Request.id
                    })
                    .ToList();
            }
            else if (type == "Hotel")
            {
                requests = _context.Hotels
                    .Join(_context.VerificationRequests,
                          h => h.id,
                          v => v.provider_Id,
                          (h, v) => new { Hotel = h, Request = v })
                    .Where(joined => joined.Request.role == "Hotel")
                    .Select(joined => new VerificationViewModel
                    {
                        providerId = joined.Hotel.id,
                        name = joined.Hotel.name,
                        email = joined.Hotel.email,
                        pdf = joined.Hotel.verificationDocuments,
                        type = "Hotel",
                        id = joined.Request.id
                    })
                    .ToList();
            }
            else if (type == "Restaurant")
            {
                requests = _context.Restaurants
                    .Join(_context.VerificationRequests,
                          r => r.id,
                          v => v.provider_Id,
                          (r, v) => new { Restaurant = r, Request = v })
                    .Where(joined => joined.Request.role == "Restaurant")
                    .Select(joined => new VerificationViewModel
                    {
                        providerId = joined.Restaurant.id,
                        name = joined.Restaurant.name,
                        email = joined.Restaurant.email,
                        pdf = joined.Restaurant.verificationDocuments,
                        type = "Restaurant",
                        id = joined.Request.id
                    })
                    .ToList();
            }
            return requests;
        }

        public List<ServiceRequestsViewModel> GetServiceRequestsViewModels(string role)
        {
            if (role == "Merchant")
            {
                return (from s in _context.ServiceRequests
                        join p in _context.Products
                        on s.serviceId equals p.id
                        where s.role == role
                        select new ServiceRequestsViewModel
                        {
                            id = s.id,
                            providerId = p.merchantId,
                            serviceId = p.id,
                            service_name = p.name,
                            date = p.dateAdded,
                            serviceType = "Merchant"

                        }).ToList();
            }
            else if (role == "Hotel")
            {
                return (from s in _context.ServiceRequests
                        join r in _context.Rooms
                        on s.serviceId equals r.id
                        where s.role == "Hotel"
                        select new ServiceRequestsViewModel
                        {
                            id = s.id,
                            providerId = r.hotelId,
                            serviceId = r.id,
                            service_name = "Hotel",
                            serviceType = "Hotel",
                            date = r.dateAdded
                        }).ToList();
            }
            else if (role == "TourGuide")
            {
                return (from s in _context.ServiceRequests
                        join t in _context.Trips
                        on s.serviceId equals t.id
                        where s.role == "TourGuide"
                        select new ServiceRequestsViewModel
                        {
                            id = s.id,
                            providerId = t.tourGuideId,
                            serviceId = t.id,
                            service_name = t.name,
                            serviceType = "TourGuide",
                            date = t.dateAdded
                        }).ToList();
            }
            else if (role == "Restaurant")
            {
                return (from s in _context.ServiceRequests
                        join m in _context.Meals
                        on s.serviceId equals m.id
                        where s.role == "Restaurant"
                        select new ServiceRequestsViewModel
                        {
                            id = s.id,
                            providerId = m.restaurantId,
                            serviceId = m.id,
                            service_name = m.name,
                            date = m.dateAdded,
                            serviceType = "Restaurant"
                        }).ToList();
            }
            return null;
        }
        public ServiceRequest GetServiceRequest(int serviceId, string role)
        {
            return _context.ServiceRequests.FirstOrDefault(s => s.serviceId == serviceId && s.role == role);
        }
        public void ShipProduct(int orderId)
        {
            var order = _context.TouristProducts.Find(orderId);
            if (order != null)
            {
                order.status = "Delivered";
                _context.SaveChanges();
            }
        }

        public void RefreshEcommerceTransactions()
        {
            // 1. Calculate the cutoff date in C# (14 days ago).
            // This part is calculated locally.
            DateTime cutoffDate = DateTime.Today.AddDays(-14);

            // 2. Rewrite the LINQ query to compare the database date 
            //    against the C# cutoffDate. EF Core can translate this.
            List<TouristProduct> orders = _context.TouristProducts
                .Where(tp => tp.funded == false && tp.arrivalDate.HasValue && tp.arrivalDate.Value < cutoffDate)
                .ToList();

            // Check if the central credit card exists before proceeding
            var cc = _context.CreditCards.Find("00000000000000");

            if (cc == null)
            {
                // Handle case where central credit card is not found
                // Perhaps log an error or throw an exception
                return;
            }

            // Existing logic for processing orders
            foreach (var tp in orders)
            {
                // Null checks for related entities are highly recommended here
                var tourist = _context.Tourists.Find(tp.touristId);
                var product = _context.Products.Find(tp.productId);

                // Ensure product and merchant exist before funding
                if (product == null) continue;

                var merchant = _context.Merchants.Find(product.merchantId);

                if (merchant?.creditCard == null) continue; // Must have a credit card to receive funds

                // The original calculation is correct
                decimal cost = (decimal)product.price * (decimal)(1 - product.offer / 100) * tp.amount;

                merchant.creditCard.Balance += cost;
                cc.Balance -= cost;
                tp.funded = true;

                var msg = new InboxMsg
                {
                    providerType = "Merchant",
                    content = $"The transaction for Order (Product: {product.name}, Amount: {tp.amount}, Customer: {tourist?.firstName ?? "Unknown"}) has been completed successfully.",
                    providerId = tp.product.merchantId,
                    date = DateTime.Now
                };
                _context.InboxMsgs.Add(msg);
            }
            _context.SaveChanges();
        }
    }
}
