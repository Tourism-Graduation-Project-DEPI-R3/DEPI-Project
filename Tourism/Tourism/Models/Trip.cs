using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json;
using Tourism.Models;
using Tourism.Models.Relations;

namespace Tourism.Models
{
    public class Trip
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Trip name is required")]
        [StringLength(100, ErrorMessage = "Trip name cannot exceed 100 characters")]
        public string name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
        public string description { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(100, ErrorMessage = "Destination cannot exceed 100 characters")]
        public string destination { get; set; }

        public string triptype { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal cost { get; set; }

        public bool status { get; set; } = true;

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        // Capacity and RemainingSeats for booking system
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Remaining seats cannot be negative")]
        public int RemainingSeats { get; set; }

        // Main image as bytes
        [Display(Name = "Main Trip Image")]
        public byte[] MainImage { get; set; }

        [Required(ErrorMessage = "You must upload at least 3 images")]
        public string TripSecondaryImagesJson { get; set; }

        [NotMapped]
        public List<byte[]> Images
        {
            get
            {
                if (string.IsNullOrEmpty(TripSecondaryImagesJson))
                    return new List<byte[]>();

                return JsonSerializer.Deserialize<List<byte[]>>(TripSecondaryImagesJson)
                       ?? new List<byte[]>();
            }
            set
            {
                TripSecondaryImagesJson = JsonSerializer.Serialize(value);
            }
        }

        public List<TourPlan> TourPlans { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day")]
        public int Duration { get; set; }

        [NotMapped]
        public DateTime CalculatedEndDate
        {
            get
            {
                return StartDate.AddDays(Duration);
            }
        }

        [Required(ErrorMessage = "Tour guide is required")]
        public int tourGuideId { get; set; }

        [ForeignKey(nameof(tourGuideId))]
        [ValidateNever]
        public TourGuide TourGuide { get; set; }

        public bool accepted { get; set; } = false;

        [Required]
        public DateTime dateAdded { get; set; }

        public ICollection<TouristCart> TouristCarts { get; set; } = new List<TouristCart>();
    }
}
