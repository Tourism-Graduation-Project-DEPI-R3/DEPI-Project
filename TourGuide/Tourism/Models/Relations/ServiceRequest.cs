using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.Relations
{
    public class ServiceRequest
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int serviceId { get; set; }

        [Required]
        public string role { get; set; }


    }
}
