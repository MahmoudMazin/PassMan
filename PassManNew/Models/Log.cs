using PassManNew.Models.Shared;
using PassManNew.Models;
using PassManNew.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Identity;


namespace PassManNew.Models
{
    [Table("Log")]
    public class Log
    {

        public Guid Id { get; set; }

        [Display(Name = "Log Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LogType LogType { get; set; }

        [Display(Name = "Log")]
        public string Description { get; set; }

        [Display(Name = "Date-Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:MM}")]
        public DateTimeOffset LogDateTime { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }


    }

    public class LogExportModel
    {

        [Display(Name = "Log Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LogType LogType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

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


        [Display(Name = "Date-Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:MM}")]
        public DateTimeOffset LogDateTime { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }


    }

    public class MyLog : IMyLog {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MyLog(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public void CreateLog(LogType Type, string Log, string userName)
        {
            Log temp = new Log();
            temp.LogDateTime = System.DateTime.Now;
            temp.Description = Log;
            temp.LogType = Type;
            temp.UserName = userName; 
           
            _context.Add(temp);
            _context.SaveChanges();
        }
    }

    public interface IMyLog
    {
        void CreateLog(LogType Type, string Log, string userId);
        
    }

}
