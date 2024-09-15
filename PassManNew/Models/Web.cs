using PassManNew.Models.Shared;
using PassManNew.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PassManNew.Models
{
    [Table("Web")]
    public class Web
    {
                           

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string WebName { get; set; }

        
        [Required(ErrorMessage = "The field {0} is required.")]
        public string JsonRPA { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        public string UserNameField { get; set; }


        [Required(ErrorMessage = "The field {0} is required.")]
        public string PwdNameField { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Link> Links { get; set; }


    }

    public partial class WebViewModel
    {

      

        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Web Name")]
        public string WebName { get; set; }

        [Display(Name = "Json ")]
        public string JsonRPA { get; set; }

        [Display(Name = "User Name Field Id")]
        public string UserNameField { get; set; }

        [Display(Name = "Password Field Id")]
        public string PwdNameField { get; set; }
    }


}
