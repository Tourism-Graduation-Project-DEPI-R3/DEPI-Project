using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tourism.Models;

public class CreditCard
{
    [Key]
    [StringLength(16)]
    public string CardNumber { get; set; }

    [Required]
    [Precision(12, 4)]
    public decimal Balance { get; set; }

    [Required]
    public string CardHolder { get; set; }

    // FK to TourGuide
    public int? UserId { get; set; }          // nullable if card may be unlinked
    public TourGuide TourGuide { get; set; }  // navigation property

    [Required]
    public string ExpiryDate { get; set; }

    [Required]
    public string CVV { get; set; }
}
