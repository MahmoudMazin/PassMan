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
    [Table("Link")]
    public class Link
    {
       
        public Guid Id { get; set; }               

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100)]
        public string LinkName { get; set; }

        [Display(Name = "Logo")]
        public byte[]? Logo { get; set; }


        [Required(ErrorMessage = "The field {0} is required.")]
        public string WebId { get; set; }


        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid ServiceId { get; set; }


        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

       
        [StringLength(255)]
        [Required(ErrorMessage = "The field {0} is required.")]
        public string LinkUserName { get; set; }

       
        [StringLength(255)]
        [Required(ErrorMessage = "The field {0} is required.")]
        public string LinkUserPassword { get; set; }

     
        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }
                
        public virtual Service Service { get; set; }

        public virtual Web Web { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LinkPermission> LinkPermissions { get; set; }

    }


    public partial class LinkViewModel
    {

        public Guid Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Link Name")]
        public string LinkName { get; set; }

        [Display(Name = "Web")]
        public string WebName { get; set; }


        [Display(Name = "Web")]
        public string WebId { get; set; }
         

        [Display(Name = "Description")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        public string Description { get; set; }

        [Display(Name = "User Name")]
        [StringLength(255)]
        [Required(ErrorMessage = "The field {0} is required.")]
        public string LinkUserName { get; set; }

        [Display(Name = "Password")]
        [StringLength(255)]
        [Required(ErrorMessage = "The field {0} is required.")]
        public string LinkUserPassword { get; set; }

        

        [Display(Name = "Logo")]
        public IFormFile Logo { get; set; }

        public byte[] ExistingLogo { get; set; }

        [Display(Name = "Remove Image?")]
        public bool RemoveImage { get; set; }

        [Display(Name = "Service ID")]
        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid ServiceId { get; set; }

        [Display(Name = "Service Name")]
        public string ServiceName { get; set; }

        [Display(Name = "Section ID")]
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


        [Display(Name = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

        [Display(Name = "Authorize Users")]
        public IEnumerable<string> AuthorizeUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LinkPermission> LinkPermissions { get; set; }

    }


    public partial class RPAJsonModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RPAJsonModel()
        {
            Commands = new HashSet<Commands>();
        }
        public string Name { get; set; }
        public string CreationDate { get; set; }
        public ICollection<Commands> Commands { get; set; }
    }

    public partial class Commands
    {
        public string Command { get; set; }
        public string Target { get; set; }
        public string Value { get; set; }
        public List<string> Targets { get; set; }
        public string Description { get; set; }
    }

    [Table("LinkPermission")]
    public class LinkPermission
    {
        [Required(ErrorMessage = "The field {0} is required.")]
        public Guid LinkId { get; set; }

        [StringLength(450)]
        [Required(ErrorMessage = "The field {0} is required.")]
        public string PersonId { get; set; }

        public virtual ApplicationUser Person { get; set; }

        public virtual Link Link { get; set; }

    }

    public class UsersPerLink {

        public Guid LinkId { get; set; }

        public string PersonId { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Display(Name = "Dept")]
        public string Dept { get; set; }

        [Display(Name = "Section")]
        public string Section { get; set; }

        [Display(Name = "Service")]
        public string Service { get; set; }

        [Display(Name = "Link")]
        public string Link { get; set; }

        [Display(Name = "Web")]
        public string Web { get; set; }
    }

}
