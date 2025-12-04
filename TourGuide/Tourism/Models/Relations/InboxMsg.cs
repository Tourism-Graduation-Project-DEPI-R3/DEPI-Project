using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.Relations
{
    public class InboxMsg
    {
        [Key]
        public int id { get; set; }


        public int providerId { get; set; }
        public string providerType { get; set; }

        [Required]
        public string content { get; set; }

        [Required]
        public DateTime date { get; set; }

    }
}
