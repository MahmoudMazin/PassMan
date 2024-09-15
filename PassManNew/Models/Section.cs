using PassManNew.Models.Shared;
using PassManNew.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace PassManNew.Models
{
    [Table("Section")]
    public class Section
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Section()
        {
             Services = new HashSet<Service>();            
        }

        public Guid Id { get; set; }
               

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100)]
        public string SectionName { get; set; }

        [Display(Name = "Logo")]
        public byte[]? Logo { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid DeptId { get; set; }


        [StringLength(255)]
        public string? Description { get; set; }

            
        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

        
        public virtual Dept Dept { get; set; }

  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Service> Services { get; set; }

    }


    public partial class SectionViewModel
    {

        public Guid Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Section Name")]
        public string SectionName { get; set; }

        [Display(Name = "Logo")]
        public IFormFile Logo { get; set; }

        public byte[] ExistingLogo { get; set; }

        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }

        [Display(Name = "Dept ID")]
        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid DeptId { get; set; }

        [Display(Name = "Dept Name")]
        public string DeptName { get; set; }

        [Display(Name = "Company ID")]
        public Guid CompanyId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Description")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        public string Description { get; set; }
        
                                
        [Display(Name = "Services")]
        public int Services { get; set; }

        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }


    }


    public class CustomListItem {

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
