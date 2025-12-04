using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.IRepository
{
    public interface IProductRepository
    {
        public Product MerchantProduct(int productId, int merchantId);

        public List<Product> GetProducts(int merchantId);

        public List<ProductViewModel> GetProductsViewModel(int merchantId);
        public List<Image> GetPics(int productId);
        public List<ProductViewModel> MergeProductToViewModel(List<Product> products);

        public List<ProductViewModel> NewArrivals();
        public List<ProductViewModel> BestSellers();
        public List<ProductViewModel> GetByCategory(string category);
        public ProductViewModel MergeProductToViewModel(Product product);
        public List<ProductViewModel> GetCartProducts(int touristId);
        public List<ProductViewModel> GetBySearch(string search);
        public List<ProductViewModel> GetByFilter(List<ProductViewModel> allProducts, double? minPrice, double? maxPrice, string? sort);
    }
}
