using System.ComponentModel.DataAnnotations;

namespace Tourism.Models
{
    public class Admin
    {
        [Key]
        public int id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]   // Prevents nvarchar(max) and uses VARCHAR(255) in MySQL
        public string email { get; set; }

        [Required]
        [MaxLength(500)]   // Safe size for hashed passwords
        public string passwordHash { get; set; }
    }
}
