using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tourism.Models.Relations;
using System.ComponentModel;

namespace Tourism.Models
{
    public class Product
    {
        [Key]
        public int id { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int count { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; }

        [Required]
        [StringLength(1000)]
        public string description { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public double price { get; set; }

        [Range(0, 100)]
      
        public double offer { get; set; }




       
        [ForeignKey(nameof(merchant))]
        public int merchantId { get; set; }
        public Merchant merchant { get; set; }
        public List<FavouriteProduct> favourites { get; set; } = new();
        public List<CartProduct> cart { get; set; } = new();

        [Required]
        public string category { get; set; }
        public bool accepted { get; set; } = false;

        [Required]
        public DateTime dateAdded { get; set; }

        [Required]
        public int DeliversWithin { get; set; } = 0;
    }
}
