using System.ComponentModel.DataAnnotations;

namespace PassManNew.Models
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Username/Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
