using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models
{
    public class TouristCart
    {
        public int Id { get; set; }
        // Identity user id (string) — add this
        public string UserId { get; set; }
        public int TouristId { get; set; }
        public int TripId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [ValidateNever]
        public Trip Trip { get; set; }

        [ValidateNever]
        public Tourist Tourist { get; set; }
    }
}
