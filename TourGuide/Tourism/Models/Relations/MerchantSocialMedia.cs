using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.Relations
{
    public class MerchantSocialMedia
    {
        [Key]
        public int Id { get; set; }

        [Required, Url]
        public string Link { get; set; }

        [ForeignKey(nameof(Merchant))]
        public int MerchantId { get; set; }
        public Merchant Merchant { get; set; }
    }
}
