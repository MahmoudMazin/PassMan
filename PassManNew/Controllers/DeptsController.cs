
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
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.IO;
using PassManNew.Resources;

namespace PassManNew.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LocalizationService _localizationService;

        public DeptsController(LocalizationService localizationService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _localizationService = localizationService;
        }

        // GET: Depts
        public IActionResult Index(string id)
        {
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", id);
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
                IQueryable<Dept> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<DeptViewModel> DeptViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = DeptViewModels });

            }
            catch (Exception)
            {
                throw;
            }


        }

        private List<DeptViewModel> GetViewModel(IQueryable<Dept> Depts)
        {
            List<DeptViewModel> DeptViewModels = new List<DeptViewModel>();

            foreach (Dept item in Depts)
            {
                DeptViewModel curitem = new DeptViewModel();
                curitem.Id = item.Id;
                curitem.DeptName = item.DeptName;
                curitem.Description = item.Description;
                curitem.ExistingLogo = item.Logo;               
                curitem.CompanyName = item.Company.CompanyName;                
                curitem.Sections = item.Sections.Where(u=>u.State == Models.Shared.State.Active).Count();            
                curitem.State = item.State;

                DeptViewModels.Add(curitem);
            }

            return DeptViewModels;
        }

        private IQueryable<Dept> GetData(JqueryDataTablesParameters param)
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
                string CompanyFilter = AdditionalValues[1].ToString();

                // getting all Customer data  
                IQueryable<Dept> data = Enumerable.Empty<Dept>().AsQueryable();

                data = _context.Depts
                    .Include(r => r.Sections)
                    .Include(r => r.Company);
                  

                if(CompanyFilter != "All")
                { 
                  data= data.Where(u => u.CompanyId.ToString() == CompanyFilter); ;
                }
               

                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    sortColumn = sortColumn.Replace(" DESC", "");            
                    switch (sortColumn)
                    {
                        case "DeptManagerName":
                            sortColumn = "DeptManager.PersonName";
                            break;
                        case "CompanyName":
                            sortColumn = "Company.CompanyName";
                            break;
                        case "Sections":
                            sortColumn = "id";
                            break;
                        default:
                            // code block
                            break;
                    }
                    sortColumn += " " + sortColumnDirection.ToString();

                    data = data.OrderBy(sortColumn );
                }

                //State  
                if (!string.IsNullOrEmpty(StateFiler))
                {
                    if (StateFiler == "true")
                        data = data.Where(m => m.State == Models.Shared.State.Active);

                }

                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.DeptName.ToLower().Contains(searchValue.ToLower())
                    || (m.Description != null && m.Description.ToString().ToLower().Contains(searchValue.ToLower()))
                    || m.Company.CompanyName.ToLower().Contains(searchValue.ToLower()) 
                    || m.State.ToString().ToLower().Contains(searchValue.ToLower())                                     
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

            List<DeptViewModel> DeptViewModels = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            //Returning Json Data  
            return Json(new { data = DeptViewModels });

        }




       
        public async Task<string> Delete(Guid Id)
        {

            var data = _context.Depts.Include(u => u.Sections).Where(u => u.Id == Id).FirstOrDefault();

            var childcounts = data.Sections.Where(u => u.State == Models.Shared.State.Active).Count();

            // here we need to write logic for checking all related record before Inactive.
            if (data.State == Models.Shared.State.Active && childcounts > 0)
            {
                return string.Format(_localizationService.GetLocalized("Unable to Inactive this Dept as {0} Sections are active in this Dept. Please inactivate all Sections and then try again."), childcounts);
            }

            data.State = data.State == Models.Shared.State.Active ? Models.Shared.State.InActive : Models.Shared.State.Active;

            try
            {
                _context.Update(data);               
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return _localizationService.GetLocalized("Error: ") + ex.Message;
            }

            return "Succeeded";
        }

        // GET: Depts/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Depts
                .Include(r => r.Company)                
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            DeptViewModel curitem = new DeptViewModel();       

            curitem.Id = item.Id;
            curitem.CompanyId = item.Company.Id;
            curitem.DeptName = item.DeptName;
            curitem.Description= item.Description;
            curitem.ExistingLogo = item.Logo;
            curitem.CompanyName = item.Company.CompanyName;           
            curitem.State = item.State;                        
            return View(curitem);
        }

        // GET: Depts/Create       
        public IActionResult Create(string id)
        {        
            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State== Models.Shared.State.Active), "Id", "CompanyName",id);
            return View();
        }

        // POST: Depts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]      
        public async Task<IActionResult> Create(DeptViewModel item)
        {
            if (ModelState.IsValid)
            {
                Dept curitem = new Dept();
                curitem.Id = new Guid();
                curitem.DeptName = item.DeptName;
                curitem.Description = item.Description;
                curitem.CompanyId = item.CompanyId;
                curitem.State = Models.Shared.State.Active;

                // Logo Upload
                using (var memoryStream = new MemoryStream())
                {
                    if (item.Logo != null)
                    {
                        bool validate = true;
                        if (item.Logo.ContentType != "image/jpeg")
                        {
                            ModelState.AddModelError("File", _localizationService.GetLocalized("The File Type is not permitted. Only Jpeg format is allowed."));
                            validate = false;
                        }
                        if (item.Logo.Length > (100 * 1024)) // 5KB
                        {
                            ModelState.AddModelError("File", _localizationService.GetLocalized("The file is too large. Maximum allowed size is 100 KB."));
                            validate = false;
                        }

                        if (validate)
                        {
                            item.Logo.CopyTo(memoryStream);
                            curitem.Logo = memoryStream.ToArray();
                        }
                        else
                        {
                            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
                             return View(item);
                        }
                    }
                }
                _context.Add(curitem);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),new { id = curitem.CompanyId.ToString() });
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName",item.CompanyId);
            return View(item);
        }

        // GET: Depts/Edit/5       
        public  IActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = _context.Depts
                .Include(r => r.Company)
                .Where(u=>u.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            DeptViewModel curitem = new DeptViewModel();
            curitem.Id = item.Id;
            curitem.DeptName = item.DeptName;
            curitem.Description = item.Description;
            curitem.CompanyId = item.CompanyId;
            curitem.State = item.State;
            curitem.ExistingLogo = item.Logo;

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            return View(curitem);
        }


        // POST: Depts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(DeptViewModel item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    // To remove Old SalesMan from Saleman role
                    Dept curitem = _context.Depts.Where(u => u.Id == item.Id).FirstOrDefault();
                 
                    curitem.Id = item.Id;
                    curitem.DeptName = item.DeptName;
                    curitem.Description = item.Description;
                    curitem.CompanyId = item.CompanyId;
                    curitem.State = item.State;

                    if (item.RemoveImage)
                    {
                        curitem.Logo = null;
                    }

                    // Logo Upload
                    using (var memoryStream = new MemoryStream())
                    {
                        if (item.Logo != null)
                        {
                            bool validate = true;
                            if (item.Logo.ContentType != "image/jpeg")
                            {
                                ModelState.AddModelError("File", _localizationService.GetLocalized("The File Type is not permitted. Only Jpeg format is allowed."));
                                validate = false;
                            }
                            if (item.Logo.Length > (100 * 1024)) // 5KB
                            {
                                ModelState.AddModelError("File", _localizationService.GetLocalized("The file is too large. Maximum allowed size is 100 KB."));
                                validate = false;
                            }

                            if (validate)
                            {
                                item.Logo.CopyTo(memoryStream);
                                curitem.Logo = memoryStream.ToArray();
                            }
                            else
                            {
                                ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
                                 return View(item);
                            }
                        }
                    }

                    _context.Update(curitem);

                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
                return RedirectToAction(nameof(Index), new { id = item.CompanyId.ToString() });
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            return View(item);
        }



        private bool DeptExists(Guid id)
        {
            return _context.Depts.Any(e => e.Id == id);
        }
    }
}
