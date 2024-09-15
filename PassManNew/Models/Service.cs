using PassManNew.Models.Shared;
using PassManNew.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



namespace PassManNew.Models
{
    [Table("Service")]
    public class Service
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Service()
        {
            Links = new HashSet<Link>();
        }
        public Guid Id { get; set; }               

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100)]
        public string ServiceName { get; set; }

        [Display(Name = "Logo")]
        public byte[]? Logo { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid SectionId { get; set; }


        [StringLength(255)]
        public string? Description { get; set; }
                
        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }
                
        public virtual Section Section { get; set; }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Link> Links { get; set; }

    }


    public partial class ServiceViewModel
    {

        public Guid Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; }

        [Display(Name = "Description")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        public string Description { get; set; }

        [Display(Name = "Logo")]
        public IFormFile Logo { get; set; }

        public byte[] ExistingLogo { get; set; }

        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }

        [Display(Name = "Section ID")]
        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid SectionId { get; set; }

        [Display(Name = "Section Name")]
        public string SectionName { get; set; }

        [Display(Name = "Dept ID")]        
        public Guid DeptId { get; set; }

        [Display(Name = "Dept Name")]
        public string DeptName { get; set; }


        [Display(Name = "Company ID")]
        public Guid CompanyId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        
        [Display(Name = "Links")]
        public int Links { get; set; }

        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }
        
    }
}
