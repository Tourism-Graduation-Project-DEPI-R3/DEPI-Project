using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Card Number is required")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Card Holder Name is required")]
        public string CardHolder { get; set; }

        [Required(ErrorMessage = "Expiry Date is required (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be 3 digits")]
        public string CVV { get; set; }
    }

    public class CartItemViewModel
    {
        public int TripId { get; set; }
        public string TripName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}