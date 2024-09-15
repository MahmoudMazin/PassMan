using PassManNew.Models.Shared;
using PassManNew.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PassManNew.Models
{

    public partial class CategoryNavViewModel
    {
        public string Category { get; set; }

   
        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Logo")]
        public byte[] Logo { get; set; }

        public string NextNav { get; set; }

        public string TabTitle { get; set; }

        public string TabColor { get; set; }

        public string TabIcon { get; set; }

        public string Breadcumb { get; set; }

    }


    public class Dashboard {


        public Dashboard()
        {
    
        }


    }

}
