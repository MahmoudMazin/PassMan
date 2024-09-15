
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PassManNew.Models.Shared;

namespace PassManNew.Models
{
    [Table("Company")]
    public partial class Company
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Company()
        {
            Depts = new HashSet<Dept>();           
        }

        public Guid Id { get; set; }

        
        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(255)]
        public string CompanyName { get; set; }


        [StringLength(255)]
        public string Address { get; set; }

        
        [Display(Name = "Logo")]
        public byte[]? Logo { get; set; }
      

        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

                
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Dept> Depts { get; set; }
       
    }




    public partial class CompanyViewModel
    {
        public Guid Id { get; set; }


        [Required(ErrorMessage = "Required")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

       
        [StringLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; }
              
        [Display(Name = "Logo")]
        public IFormFile Logo { get; set; }

        public byte[] ExistingLogo { get; set; }


        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }

       

        
        [Display(Name = "Depts")]
        public int Depts { get; set; }


        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

    }
}

