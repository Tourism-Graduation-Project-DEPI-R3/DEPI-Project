using AspNetCoreGeneratedDocument;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.Repository
{
    public class ProductRepository:IProductRepository
    {
        private readonly TourismDbContext _context;

        public ProductRepository(TourismDbContext context)
        {
            _context = context;
        }
        public Product MerchantProduct(int productId,int merchantId)
        {
            return _context.Products.FirstOrDefault(p => p.id == productId && p.merchantId == merchantId);
        }
        public List<Product> GetProducts(int merchantId)
        {
            return (
                from p in _context.Products
                where p.merchantId == merchantId
                select p
            ).ToList();
        }
        public List<ProductViewModel> GetProductsViewModel(int merchantId)
        {
            return (
                from p in _context.Products
                where p.merchantId == merchantId
                select new ProductViewModel
                {
                    id = p.id,
                    name = p.name,
                    count = p.count,
                    price = p.price,
                    accepted = p.accepted
                }
            ).ToList();
        }
        public List<Image> GetPics(int productId)
        {
            return _context.Images.Where(i => i.serviceType == "Product" && i.serviceId == productId).ToList();
        }

        public List<ProductViewModel> MergeProductToViewModel(List<Product> products)
        {
            List<ProductViewModel> ret = new();
            foreach (var p in products)
            {
                ProductViewModel productModel = new();
                productModel.id = p.merchantId;
                productModel.productId = p.id;
                productModel.name = p.name;
                productModel.count = p.count;
                productModel.price = p.price;
                productModel.description = p.description;
                productModel.offer = p.offer;
                productModel.rate = (int?)_context.TouristFeedbacks
                                          .Where(tf => tf.targetId == p.id && tf.targetType == "Product")
                                          .Select(tf => (double?)tf.rating) 
                                          .Average() ?? 0;
                productModel.DeliversWithin = p.DeliversWithin;
                productModel.deliveryDate = DateTime.Today.AddDays(p.DeliversWithin);
                List<Image> imgs = _context.Images.Where(i => i.serviceId == p.id && i.serviceType == "Product").ToList();
                int index = 0;
                foreach (var img in imgs)
                {
                    if (img.imageData != null && img.imageData.Length > 0)
                    {
                        var stream = new MemoryStream(img.imageData);

                        var file = new FormFile(stream, 0, img.imageData.Length, $"image{index}", $"image{index}.jpg");

                        productModel.UploadedImages.Add(file);

                        index++;
                    }

                }
                
                ret.Add(productModel);
            }
            return ret;
        }

        public ProductViewModel MergeProductToViewModel(Product product)
        {
            ProductViewModel ret = new();
            ret.id = product.merchantId;
            ret.productId = product.id;
            ret.name = product.name;
            ret.count = product.count;
            ret.price = product.price;
            ret.description = product.description;
            ret.offer = product.offer;
            ret.rate = (int?)_context.TouristFeedbacks
                                      .Where(tf => tf.targetId == product.id && tf.targetType == "Product")
                                      .Select(tf => (double?)tf.rating)
                                      .Average() ?? 0;
            ret.deliveryDate = DateTime.Today.AddDays(product.DeliversWithin);
            List<Image> imgs = _context.Images.Where(i => i.serviceId == product.id && i.serviceType == "Product").ToList();
            int index = 0;
            foreach (var img in imgs)
            {
                if (img.imageData != null && img.imageData.Length > 0)
                {
                    var stream = new MemoryStream(img.imageData);

                    var file = new FormFile(stream, 0, img.imageData.Length, $"image{index}", $"image{index}.jpg");

                    ret.UploadedImages.Add(file);

                    index++;
                }

            }
            ret.Reviews = _context.TouristFeedbacks
          .Where(tf => tf.targetId == product.id && tf.targetType == "Product")
          .Select(tf => new ReviewDto
          {
              Comment = tf.comment,
              Rating = tf.rating,
              CreatedAt = tf.createdAt
          })
          .ToList();


            return ret;
        }


        public List<ProductViewModel> NewArrivals()
        {
             List<Product>products = _context.Products
                .OrderBy(p => p.dateAdded)
                .Take(15)
                .ToList();
           
            return MergeProductToViewModel(products);
        }

        public List<ProductViewModel> BestSellers()
        {
            var products = (
                            from tp in _context.TouristProducts
                            group tp by tp.productId into g
                            select new
                            {
                                ProductId = g.Key,
                                TotalAmount = g.Sum(x => x.amount)
                            })
                            .OrderByDescending(x => x.TotalAmount)
                            .Take(15)
                            .Join(_context.Products,
                                  x => x.ProductId,
                                  p => p.id,
                                  (x, p) => p)
                            .ToList();
            return MergeProductToViewModel(products);
        }

        public List<ProductViewModel> GetByCategory(string category)
        {
            List<Product> products = _context.Products.Where(p => p.category == category).ToList();
            return MergeProductToViewModel(products);
        }

        public List<ProductViewModel> GetCartProducts(int touristId)
        {
            List<Product> products = (
              from cp in _context.CartProducts
              where cp.TouristId == touristId
              join p in _context.Products
                  on cp.ProductId equals p.id
              select p
          ).ToList();

            return MergeProductToViewModel(products);
        }

        public List<ProductViewModel> GetBySearch(string search)
        {
            search = search.ToLower();
            string term = "%";
            foreach (char c in search)
                term += c + "%";


            var products = _context.Products
                .Where(p =>
                    EF.Functions.Like(p.name.ToLower(), term)||  
                    EF.Functions.Like(p.name.ToLower(), $"%{search}%")   ||
                    EF.Functions.Like(p.name.ToLower(), $"{search}%") ||    
                    EF.Functions.Like(p.name.ToLower(), $"%{search}")||
                    EF.Functions.Like(p.name.ToLower() + 's', $"%{search}%") ||  
                    EF.Functions.Like(p.name.ToLower() + 's', $"{search}%")  ||  
                    EF.Functions.Like(p.name.ToLower() + 's', $"%{search}")      
                )
                .ToList();

            return MergeProductToViewModel(products);
        }
        public List<ProductViewModel> GetByFilter(List<ProductViewModel> allProducts, double? minPrice, double? maxPrice, string? sort)
        {


            if (minPrice.HasValue)
                allProducts = allProducts.Where(p => p.price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                allProducts = allProducts.Where(p => p.price <= maxPrice.Value).ToList();

            if (sort == "low")
                allProducts = allProducts.OrderBy(p => p.price).ToList();
            else if (sort == "high")
                allProducts = allProducts.OrderByDescending(p => p.price).ToList();
            return allProducts;
        }
    }
}
