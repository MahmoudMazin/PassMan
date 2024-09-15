using PassManNew.Models.Shared;
using PassManNew.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



namespace PassManNew.Models
{
    [Table("Dept")]
    public class Dept
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Dept()
        {
             Sections = new HashSet<Section>();            
        }
        public Guid Id { get; set; }
               

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100)]
        public string DeptName { get; set; }

        [Display(Name = "Logo")]
        public byte[]? Logo { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid CompanyId { get; set; }


        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        
        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

        
        public virtual Company Company { get; set; }

       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Section> Sections { get; set; }

    }


    public partial class DeptViewModel
    {

        public Guid Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Dept Name")]
        public string DeptName { get; set; }

        [Display(Name = "Logo")]
        public IFormFile Logo { get; set; }

        public byte[] ExistingLogo { get; set; }

        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }


        [Display(Name = "Description")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        public string Description { get; set; }
        
       

        [Display(Name = "Company ID")]
        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid CompanyId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

     

        [Display(Name = "Sections")]
        public int Sections { get; set; }

        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }


    }
}
