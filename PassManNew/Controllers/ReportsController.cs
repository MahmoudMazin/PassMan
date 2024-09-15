using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PassManNew.Data;
using PassManNew.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using PassManNew.Resources;

namespace PassManNew.Controllers
{

    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
  

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public IActionResult Index()
        {
            
            return View();
        }


        [HttpPost]       
        public IActionResult Index([FromBody]JqueryDataTablesParameters param)
        {
            try
            {
                var draw = param.Draw;

                // Skip number of Rows count  
                var start = param.Start;

                // Paging Length 10,20  
                var length = param.Length; // Request.Query["length"].FirstOrDefault();

                //Paging Size (10, 20, 50,100)  
                int pageSize = length != 0 ? Convert.ToInt32(length) : 0;

                int skip = start != 0 ? Convert.ToInt32(start) : 0;

                int recordsTotal = 0;

                //For Export
                HttpContext.Session.SetString(nameof(JqueryDataTablesParameters), JsonConvert.SerializeObject(param));

                // getting all Customer data  
                IQueryable<Log> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                //List<Log> LogViewModels = data.ToList();


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data.ToList() });

            }
            catch
            {
                throw;
            }


        }

       
        private IQueryable<Log> GetData(JqueryDataTablesParameters param)
        {
            try
            {

                // Sort Column Name  
                var sortColumn = param.SortOrder;

                // Sort Column Direction (asc, desc)  
                var sortColumnDirection = param.Order[0].Dir; ;

                // Search Value from (Search box)  
                var searchValue = param.Search.Value;
                
                List<string> AdditionalValues = param.AdditionalValues.ToList();
                var StateFiler = AdditionalValues[0].ToString();
                

                // getting all Customer data  
                IQueryable<Log> data = Enumerable.Empty<Log>().AsQueryable();

                data = _context.Logs;


                //Default Sorting
                data = data.OrderBy("LogDateTime");

                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    data = data.OrderBy(sortColumn);
                }
                
                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.LogType.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.UserName.ToLower().Contains(searchValue.ToLower())
                    || m.LogDateTime.ToString("dd-MMM-yy HH:mm").ToLower().Contains(searchValue.ToLower())
                    || m.Description.ToLower().Contains(searchValue.ToLower())
                    );
                }
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

                
        public IActionResult GetExcel()
        {
            var param = HttpContext.Session.GetString(nameof(JqueryDataTablesParameters));

            List<Log> LogViewModels = GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)).ToList();

            List<LogExportModel> em = new List<LogExportModel>();

            foreach (Log lg in LogViewModels)
            {
                LogExportModel item = new LogExportModel(); 
                string[] description = lg.Description.Split(" | ");
                if (description.Length != 1)
                {
                    item.Company = description[0];
                    item.Dept = description[1];
                    item.Section = description[2];
                    item.Service = description[3];
                    item.Link = description[4];
                }
                else {
                    item.Description = lg.Description;
                }
                
                item.LogDateTime = lg.LogDateTime;
                item.LogType = lg.LogType;
                item.UserName = lg.UserName;

                em.Add(item);
            }


            //Returning Json Data  
            return Json(new { data = em });
        }

        public string DeleteLogs()
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE Log");
            return "Succeeded";
        }

    }
}
