using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tourism.Models.Relations;

namespace Tourism.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class Tourist
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int age { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        [EmailAddress]
        [Unicode]
        public string email { get; set; }
        [StringLength(100, MinimumLength = 6)]
        [Required]
        public string passwordHash { get; set; }
        [Required]
        [StringLength(15)]
        public string firstName { get; set; }
        [Required]
        [StringLength(15)]
        public string LastName { get; set; }
        [Required]
        [StringLength(30)]
        public string nationality { get; set; }
        [Required]
        [RegularExpression(@"^\+?[1-9]\d{9,14}$",
        ErrorMessage = "Please Enter a Valid Number")]
        public string phoneNumber { get; set; }

        [Required]
        public double balance { get; set; } = 0;
        public string address { get; set; }
        public List<FavouriteProduct> favourites { get; set; } = new();
        public List<CartProduct> cart { get; set; } = new();
        public ICollection<TouristCart> TouristCarts { get; set; } = new List<TouristCart>();


    }
}
