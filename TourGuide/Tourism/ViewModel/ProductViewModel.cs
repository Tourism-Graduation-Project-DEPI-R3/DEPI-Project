using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tourism.Models.Relations;
using Tourism.Models;
using System.ComponentModel;

namespace Tourism.ViewModel
{
    public class ReviewDto
    {
        public string Comment { get; set; } = "";
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductViewModel
    {
        [Required]
        [Range(0, int.MaxValue)]
        [DisplayName(displayName: "Count")]
        public int count { get; set; }
        public int? id { get; set; }
        public string? MerchantName { get; set; }
        public int? productId { get; set; }
        public bool accepted { get; set; } = false;
        [Required]
        [StringLength(100)]
        [DisplayName(displayName: "Name")]
        public string name { get; set; }

        [Required]
        [StringLength(1000)]
        [DisplayName(displayName: "Description")]
        public string description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [DisplayName(displayName: "Price")]
        public double price { get; set; }

        [Range(0, 100)]
        [DisplayName(displayName: "Offer")]
        public double offer { get; set; }

        [Required]
        [DisplayName(displayName: "Category")]
        public string category { get; set; }

        [NotMapped]
        [Display(Name = "Images")]
        public List<IFormFile> UploadedImages { get; set; } = new();

        public List<ReviewDto>? Reviews { get; set; } = new();

        public int rate { get; set; }

        public int ? DeliversWithin { get; set; }

        public DateTime? deliveryDate { get; set; }

    }
    }
