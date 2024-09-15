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
    [Table("Doc")]
    public class Doc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
      
        public Guid Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "File Title")]
        public string FileTitle { get; set; }


        [StringLength(255)]       
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Share Type")]
        public bool IsPublic { get; set; }

        [StringLength(100)]     
        [Display(Name = "Content Type")]
        public string ContentType { get; set; }

       
        [Display(Name = "Content")]
        public byte[] Content { get; set; }

        [NotMapped]
        [Display(Name = "Size")]
        public string Size { get; set;}

        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "File Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileType FileType { get; set; }

       
        [Display(Name = "Date")]
        public DateTime UploadDateTime { get; set; }

        [StringLength(450)]       
        [Display(Name = "Owner")]
        public string OwnerId { get; set; }

        public virtual ApplicationUser Owner { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocPermission> DocPermissions { get; set; }

        [NotMapped]
        public bool UserCanModify { get; set; }      


    }

    [Table("DocPermission")]
    public class DocPermission
    {
        [Required(ErrorMessage = "The field {0} is required.")]       
        public Guid DocId { get; set; }

        [StringLength(450)]       
        [Required(ErrorMessage = "The field {0} is required.")]
        public string PersonId { get; set; }

        public virtual ApplicationUser Person { get; set; }

        public virtual Doc Doc { get; set; }

    }

    public class DocSharingViewModel {

        public DocSharingViewModel() {
            Docs = new HashSet<Doc>();           
        }
        
        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Upload")]
        public IFormFile FormFile { get; set; }

        public Doc Doc { get; set; }

        public ICollection<Doc> Docs { get; set; }

        [Display(Name = "Authorize Users")]
        public IEnumerable<string> AuthorizeUsers { get; set; }


    }
}
