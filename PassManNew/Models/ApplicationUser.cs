using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PassManNew.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {

        }

      

        [Display(Name = "Person Name")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [MaxLength(255)]
        public string PersonName { get; set; }


        [Display(Name = "State")]
        public bool IsActive { get; set; }


        [Display(Name = "Theme")]
        public string Theme { get; set; }


        [Display(Name = "User Photo")]
        public byte[] UserPhoto { get; set; }


        [Display(Name = "Remarks")]
        [MaxLength(255)]
        public string Remarks { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Doc> Docs { get; set; }



    }

    public class NewUserPasswordViewModel
    {

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


    public class UserViewModel
    {
        [Display(Name = "User Id")]
        [MaxLength(450)]
        public string Id { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long., MinimumLength = 6)")]
        public string UserName { get; set; }


        [Display(Name = "Person Name")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long., MinimumLength = 6)")]
        [RegularExpression("^[a-zA-Z]+(([',. -][a-zA-Z ])?[a-zA-Z]*)*$", ErrorMessage = "Only alphabets is allowed")]
        public string PersonName { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        [StringLength(14, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long., MinimumLength = 14)")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "The {0} must be number and Fourteen Digits only.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Remarks")]
        [MaxLength(255)]
        public string Remarks { get; set; }

        [Display(Name = "Is User Admin?")]
        public bool IsUserAdmin { get; set; }


        [Display(Name = "Email Confirmed")]
        public bool IsEmailConfirmed { get; set; }

        [Display(Name = "Phone Confirmed")]
        public bool IsPhoneNumberConfirmed { get; set; }

        [Display(Name = "LockOut Enabled")]
        public bool IsLockOutEnabled { get; set; }

        [Display(Name = "Access Failed")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "LockOut End")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:MM}")]
        public DateTimeOffset? LockOutEndTime { get; set; }


        [Display(Name = "User Photo")]
        public IFormFile UserPhoto { get; set; }


        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }

        public byte[] ExistingPhoto { get; set; }

        [Display(Name = "Theme")]
        public string Theme { get; set; }

        [Display(Name = "State")]
        public bool IsActive { get; set; }

    }

    public class NewUserViewModel
    {
        public UserViewModel user { get; set; }
        public NewUserPasswordViewModel password { get; set; }

    }

    public class UpdateUserViewModel
    {
        [Required(ErrorMessage = "The field {0} is required.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [EmailAddress]
        public string Email { get; set; }

        public string userName { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Person Full Name")]
        [RegularExpression("^[a-zA-Z]+(([',. -][a-zA-Z ])?[a-zA-Z]*)*$", ErrorMessage = "Person name can be Alphabets")]
        public string PName { get; set; }

        public bool IsEmailConfirmed { get; set; }

    }

}
