using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tourism.Models.Relations;

namespace Tourism.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class Hotel
    {
        [Key]
        public int id { get; set; }

        [EmailAddress]
        [Required]
        public string email { get; set; }

        [Required]
        public string passwordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; }

        [Required]
        [StringLength(10000)]
        public string description { get; set; }

        [Required]
        [StringLength(200)]
        public string address { get; set; }
        [Required]
        public string hotline { get; set; }

        [Required]
        public byte[] pic { get; set; }

        public List<Room>? rooms { get; set; } = new();
        public byte[]? verificationDocuments { get; set; }
        [Required]
        public CreditCard creditCard { get; set; }
        public bool verified { get; set; } = false;





        [Required]
        public string Governorate { get; set; }





    }
}
