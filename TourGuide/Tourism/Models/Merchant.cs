using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tourism.Models.Relations;

namespace Tourism.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class Merchant
    {
        [Key]
        public int id { get; set; }

        [Required, EmailAddress]
        [StringLength(100)]
        public string email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string passwordHash { get; set; }

        [Required, StringLength(50)]
        public string name { get; set; }
  
        public CreditCard ?creditCard { get; set; }

        [Required]
        [Phone]
        public string phoneNumber { get; set; }
        
        public List<MerchantSocialMedia>? SocialMediaLinks { get; set; } = new();

        public List<Product>? Products { get; set; } = new();

        public byte[]? verificationDocuments { get; set; }
        public bool verified { get; set; } = false;

        [Required(ErrorMessage = "Zip code is required.")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid zip code format.")]
        public string zipCode { get; set; }
    }
}
