using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.Repository
{
    public class TouristRepository:ITouristRepository
    {
        private readonly TourismDbContext _context;

        public TouristRepository(TourismDbContext context)
        {
            _context = context;
        }
        public Tourist GetByEmail(string email)
        {
            return _context.Tourists.FirstOrDefault(t => t.email == email);
        }
        public double TotalEearningsRestaurants()
        {
            return ((
                from tb in _context.Tables
                from tr in _context.TouristRestaurants
                where tb.restaurantId == tr.restaurantId && tb.number == tr.tableNumber
                select (double?)tb.bookingPrice
            ).Sum() ?? 0);
        }

        public double TotalEearningsProducts()
        {
            return ((
                from tp in _context.TouristProducts
                join p in _context.Products on tp.productId equals p.id
                where tp.status == "Delivered" || tp.status=="Processing"
                select (double?)p.price
            ).Sum() ?? 0);
        }

        public double TotalEearningsHotels()
        {
            return ((
                from r in _context.Rooms
                join tr in _context.TouristRooms on r.id equals tr.roomId
                select (double?)r.cost
            ).Sum() ?? 0);
        }
        public double TotalEearningsTrips()
        {
            return ((
                from tb in _context.Tables
                from tr in _context.TouristRestaurants
                where tb.restaurantId == tr.restaurantId && tb.number == tr.tableNumber
                select (double?)tb.bookingPrice
            ).Sum() ?? 0);
        }
      
        public bool AddToCart(int productId,int touristId)
        {
            CartProduct c = new();
            c.ProductId = productId;
            c.Product =  _context.Products.Find(productId);
            if (c.Product == null)
                return false;
            
            c.TouristId = touristId;
            c.Tourist = _context.Tourists.Find(touristId);
            if (c.Tourist == null)
                return false;

            var existingCartItem = _context.CartProducts
       .FirstOrDefault(c => c.ProductId == productId && c.TouristId == touristId);
            if (existingCartItem == null)
            {
                _context.CartProducts.Add(c);
                _context.SaveChanges();
            }
            return true;
        }
        public void transaction(CreditCard cardfromrequest, decimal money, string operation, int id)
        {
            var mainCC = _context.CreditCards.Find("00000000000000");
            var tourist = _context.Tourists.FirstOrDefault(t => t.id == id);
            if (operation == "withdraw")
            {
                cardfromrequest.Balance += money;
                tourist.balance -= (double)money;
                mainCC.Balance -= money;
            }
            else
            {
                cardfromrequest.Balance -= money;
                tourist.balance += (double)money;
                mainCC.Balance += money;
            }
            _context.SaveChanges();
        }

        public void BuyProduct(int touristId,bool buyCredit,CreditCard? cc,ProductViewModel product,string location)
        {
            var tourist = _context.Tourists.Find(touristId);
            if (buyCredit==true) cc.Balance -= (decimal) product.price*product.count;
            else
            {
                tourist.balance -= product.price*product.count;
            }
            var creditcard = _context.CreditCards.Find("00000000000000");
            creditcard.Balance += (decimal) product.price * product.count;

            TouristProduct tp = new();
            tp.product = _context.Products.Find(product.productId);
            tp.tourist = tourist;
            tp.productId = (int)product.productId;
            tp.touristId = touristId;
            tp.amount = product.count;
            tp.status = "Processing";
            tp.address = location;
            tp.funded = false;
            tp.price = (Decimal) product.price;
            _context.TouristProducts.Add(tp);

            var p = _context.Products.Find(product.productId);
            p.count--;

            var msg = new InboxMsg
            {
                providerType = "Merchant",
                content = $"{tourist.firstName} has ordered {product.count} unit(s) of {product.name}.",
                providerId = (int)product.id,
                date = DateTime.Now
            };
            _context.InboxMsgs.Add(msg);

            var cp = _context.CartProducts.FirstOrDefault(c => c.ProductId == product.productId && c.TouristId == touristId);
            if (cp != null) _context.CartProducts.Remove(cp);
            if(p!=null) p.count--;
            _context.SaveChanges();
        }

        public void RemoveFromCart(int productId,int touristId)
        {
            var record = _context.CartProducts.FirstOrDefault(cp => cp.ProductId == productId && cp.TouristId == touristId);
            if (record != null)
            {
                _context.CartProducts.Remove(record);
                _context.SaveChanges();
            }
        }

        public void CancelOrder(int orderId)
        {
            var order = _context.TouristProducts.Find(orderId);
            order.status = "Cancelled";
            var cc = _context.CreditCards.Find("00000000000000");
            var product = _context.Products.Find(order.productId);
            var merchant = _context.Merchants.Find(product.merchantId);
            var tourist = _context.Tourists.Find(order.touristId);
            decimal total = (decimal) product.price * (decimal) (1 - product.offer / 100) * order.amount;
            cc.Balance -= total;
            tourist.balance += (double) total;
            order.status = "Cancelled";
            var msg = new InboxMsg
            {
                providerType = "Merchant",
                content = $"The Order (Product: {product.name}, Amount: {order.amount}, Customer: {tourist.firstName}) has been cancelled.",
                providerId =merchant.id,
                date = DateTime.Now
            };
            _context.InboxMsgs.Add(msg);
            _context.SaveChanges();
        }

        public List<ProductOrdersViewModel> GetOrders(int touristId)
        {
            List<ProductOrdersViewModel> orders = (
              from tp in _context.TouristProducts
              join t in _context.Tourists on tp.touristId equals t.id
              join p in _context.Products on tp.productId equals p.id
              where tp.touristId == touristId
              select new ProductOrdersViewModel
              {
                  orderId = tp.orderId,
                  productName = p.name,
                  orderDate = tp.orderDate,
                  arrivalDate=tp.arrivalDate,
                  amount = tp.amount,
                  status = tp.status,
                  address = tp.address,
                  refund = (tp.arrivalDate==null ||((DateTime.Today - tp.arrivalDate.Value).Days <= 14))? true:false
              })
      .ToList();

            return orders;
        }


        public bool review(int touristId,int serviceId,int rate,string comment,string type)
        {

            if (type == "Product")
            {
                var check = _context.TouristProducts.FirstOrDefault(tf => tf.touristId == touristId && tf.productId == serviceId && tf.status== "Delivered");
                if (check == null) return false;
            }
            var record = _context.TouristFeedbacks.FirstOrDefault(f => f.touristId == touristId && f.targetType == type && f.targetId == serviceId);
            if (record != null) _context.TouristFeedbacks.Remove(record);

            TouristFeedback feedback = new();
            feedback.touristId = touristId;
            feedback.targetId = serviceId;
            feedback.rating = rate;
            feedback.targetType = type;
            feedback.createdAt = DateTime.Today;
            feedback.comment = comment;
            _context.TouristFeedbacks.Add(feedback);
            _context.SaveChanges();
            return true;
        }
    }
}
