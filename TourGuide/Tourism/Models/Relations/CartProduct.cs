using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models.Relations
{
    public class CartProduct
    {
        [ForeignKey(nameof(Tourist))]
        public int TouristId { get; set; }
        public Tourist Tourist { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}
