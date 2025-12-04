using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class FavouriteProduct
    {
        [ForeignKey(nameof(Tourist))]
        public int touristId { get; set; }
        public Tourist tourist { get; set; }

        [ForeignKey(nameof(Product))]
        public int productId { get; set; }
        public Product product { get; set; }
    }
}
