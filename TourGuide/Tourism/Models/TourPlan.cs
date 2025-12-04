using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models
{
    public class TourPlan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Heading is required")]
        [StringLength(200, ErrorMessage = "Heading cannot exceed 200 characters")]
        public string Heading { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        // Foreign Key to Trip
        [Required]
        public int TripId { get; set; }

        // Navigation Property
        [ForeignKey(nameof(TripId))]
        [ValidateNever]
        public Trip Trip { get; set; }
    }
}
