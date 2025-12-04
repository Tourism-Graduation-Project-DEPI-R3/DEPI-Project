using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace Tourism.Models.Relations
{
    public class VerificationRequest
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int provider_Id { get; set; }
        [Required]
        public string role { get; set; }
    }
}
