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
    public class UsersPerLinksController : Controller
    {
        private readonly ApplicationDbContext _context;
  

        public UsersPerLinksController(ApplicationDbContext context)
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
                IQueryable<UsersPerLink> data = GetData(param);

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

       
        private IQueryable<UsersPerLink> GetData(JqueryDataTablesParameters param)
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
                IQueryable<UsersPerLink> data = Enumerable.Empty<UsersPerLink>().AsQueryable();

                data = (from a in _context.LinkPermissions
                    .Include(u => u.Person)
                    .Include(u => u.Link)
                    .ThenInclude(u=>u.Web)
                    .Include(u => u.Link)
                    .ThenInclude(u => u.Service)
                    .ThenInclude(u => u.Section)
                    .ThenInclude(u => u.Dept)
                    .ThenInclude(u => u.Company)
                    
                        select new UsersPerLink()
                        {
                            PersonId = a.PersonId,
                            LinkId = a.LinkId,
                            UserName = a.Person.PersonName,
                            Company = a.Link.Service.Section.Dept.Company.CompanyName,
                            Dept = a.Link.Service.Section.Dept.DeptName,
                            Section = a.Link.Service.Section.SectionName,
                            Service = a.Link.Service.ServiceName,
                            Link = a.Link.LinkName,
                            Web = a.Link.Web.WebName
                        }).AsQueryable();
                    


                //Default Sorting
                data = data.OrderBy("UserName");

                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    data = data.OrderBy(sortColumn);
                }
                
                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.UserName.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.Company.ToLower().Contains(searchValue.ToLower())
                    || m.Dept.ToLower().Contains(searchValue.ToLower())
                    || m.Section.ToLower().Contains(searchValue.ToLower())
                    || m.Service.ToLower().Contains(searchValue.ToLower())
                    || m.Link.ToLower().Contains(searchValue.ToLower())
                    || m.Web.ToLower().Contains(searchValue.ToLower())
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

            List<UsersPerLink> LogViewModels = GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)).ToList();

            //Returning Json Data  
            return Json(new { data = LogViewModels.Select(u => new { u.UserName, u.Web, u.Link, u.Service, u.Section, u.Dept, u.Company }) });
        }

        

    }
}
