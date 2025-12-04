using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Tourism.ViewModel
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [StringLength(100)]
        public string email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string passwordHash { get; set; }

        [BindNever]
        public string ?msg { get; set; }
    }
}
