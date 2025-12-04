using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.Relations
{
    public class Image
    {
        [Key]
        public int id { get; set; }
        [Required]
        public byte[] imageData { get; set; }
        public int serviceId { get; set; }
        public string serviceType { get; set; }
    }
}
