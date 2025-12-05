using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tourism.Models;
using Tourism.Models.Relations;

namespace Tourism.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class TourGuide
    {
        [Key]
        public int TourGuideId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string passwordHash { get; set; }

        [Required(ErrorMessage = "Experience years is required")]
        [Range(0, 60, ErrorMessage = "Experience years must be between 0 and 60")]
        [Display(Name = "Years of Experience")]
        public int experienceYears { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "At least one language is required")]
        [Display(Name = "Languages Spoken")]
        public string languages { get; set; }

        [Required(ErrorMessage = "Profile picture is required")]
        [Display(Name = "Profile Picture")]
        public byte[] pic { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string gender { get; set; }

        [StringLength(50)]
        public string nationality { get; set; }

        public List<Trip> ?trips { get; set; } = new();

        public byte[] ?verificationDocuments { get; set; }
        
        public CreditCard? creditCard { get; set; }
        
        public bool verified { get; set; } = false;
        
        public ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
    }
}