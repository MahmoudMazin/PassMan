using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using System.Diagnostics;
using PassManNew.Resources;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;

namespace PassManNew.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LocalizationService _localizationService;

        public CompaniesController(LocalizationService localizationService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _localizationService = localizationService;
        }
       
        // GET: Regions
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
                IQueryable<Company> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<CompanyViewModel> CompaniesViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = CompaniesViewModels });

            }
            catch (Exception)
            {
                throw;
            }

            
        }

        private List<CompanyViewModel> GetViewModel(IQueryable<Company> Items)
        {
            List<CompanyViewModel> ViewModel = new List<CompanyViewModel>();

            foreach (Company item in Items)
            {
                CompanyViewModel curitem = new CompanyViewModel();

                curitem.Id = item.Id;
                curitem.CompanyName = item.CompanyName;
                curitem.Address = item.Address;                
                curitem.ExistingLogo = item.Logo;
                curitem.State = item.State;
                curitem.Depts = item.Depts.Where(u => u.State == Models.Shared.State.Active).Count();

                ViewModel.Add(curitem);
            }

            return ViewModel;
        }

        private IQueryable<Company> GetData(JqueryDataTablesParameters param)
        {
            try
            {

                // Sort Column Name  
                var sortColumn = param.SortOrder;  

                // Sort Column Direction (asc, desc)  
                var sortColumnDirection = param.Order[0].Dir;;

                // Search Value from (Search box)  
                var searchValue = param.Search.Value; 


                List<string> AdditionalValues = param.AdditionalValues.ToList();
                var StateFiler = AdditionalValues[0].ToString();

                // getting all Customer data  
                IQueryable<Company> data = _context.Companies.Include(r => r.Depts);
               
               
                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    sortColumn = sortColumn.Replace(" DESC", "");
                    switch (sortColumn)
                    {
                        //case "ClientManagerName":
                        //    sortColumn = "ClientProjectManager";
                        //    break;                        
                        //default:
                        //    // code block
                        //    break;
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
                    data = data.Where(m => m.CompanyName.ToLower().Contains(searchValue.ToLower())                     
                    || m.State.ToString().ToLower().Contains(searchValue.ToLower()) 
                    || m.Address.ToString().ToLower().Contains(searchValue.ToLower()));
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

            List<CompanyViewModel> ViewModels = GetViewModel( GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)) );

            //Returning Json Data  
            return Json(new { data = ViewModels});

        }

       
        public async Task<string> Delete(Guid Id)
        {

            var data = _context.Companies.Include(r=>r.Depts).Where(u => u.Id == Id).FirstOrDefault();
            var childcounts = data.Depts.Where(u => u.State == Models.Shared.State.Active).Count();
            // here we need to write logic for checking all related record before Inactive.
            if (data.State == Models.Shared.State.Active && childcounts > 0)
            {
                return string.Format(_localizationService.GetLocalized("Unable to Inactive this Company as {0} Dept(s) is active in this Company. Please inactivate all Depts and then try again."), childcounts);
            }

            data.State = data.State == Models.Shared.State.Active ? Models.Shared.State.InActive : Models.Shared.State.Active;

            try
            {
                _context.Update(data);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return _localizationService.GetLocalized("Error: ") + ex.Message;
            }            

            return "Succeeded";
        }

        // GET: Regions/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Companies                
                .Include(r => r.Depts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            
            CompanyViewModel curitem = new CompanyViewModel();
            curitem.Id = item.Id;
            curitem.CompanyName = item.CompanyName;
            curitem.Address = item.Address;          
            curitem.ExistingLogo = item.Logo;
            curitem.State = item.State;
            curitem.Depts = item.Depts.Count;

            return View(curitem);
        }

        // GET: Regions/Create
        public IActionResult Create()
        {
            // var excludeIds = new HashSet<string>(_context.Regions.Where(u=>u.State==Models.Shared.State.Active).Select(x => x.RegionalManagerId));
            //  ViewData["Id"] = new SelectList(_context.Set<ApplicationUser>().Where(x => !excludeIds.Contains(x.Id)).Where(u=>u.IsActive==true), "Id", "PersonName");

            return View();
        }

        // POST: Regions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel item)
        {
            if (ModelState.IsValid)
            {                
                Company curitem = new Company();
                curitem.Id = new Guid();
                curitem.CompanyName = item.CompanyName;
                curitem.State = Models.Shared.State.Active;
                curitem.Address = item.Address;               
                
                // Logo Upload
                using (var memoryStream = new MemoryStream())
                {
                    if (item.Logo != null)
                    {
                        bool validate=true;
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
                           return View(item);
                        }                        
                    }
                }

                _context.Add(curitem);

                //if (curitem.ClientProjectManagerId != null)
                //{
                //    var user = await _userManager.FindByIdAsync(curitem.ClientProjectManagerId);
                //    await _userManager.AddToRoleAsync(user, "ClientProjectManager");
                //}
                //if (curitem.SIDCProjectManagerId != null)
                //{
                //    var user = await _userManager.FindByIdAsync(curitem.SIDCProjectManagerId);
                //    await _userManager.AddToRoleAsync(user, "SIDCProjectManager");
                //}

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

             return View(item);
        }

        // GET: Regions/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Companies.FindAsync(id);
                                 
            if (item == null)
            {
                return NotFound();
            }

            CompanyViewModel curitem = new CompanyViewModel();

            curitem.Id = item.Id;
            curitem.CompanyName = item.CompanyName;
            curitem.State = item.State;
            curitem.Address = item.Address;
            curitem.ExistingLogo = item.Logo;

             
            return View(curitem);
        }

        // POST: Regions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyViewModel item)
        {
         
            if (ModelState.IsValid)
            {
                try
                {
                    Company data = _context.Companies.Where(u => u.Id == item.Id).FirstOrDefault();
                    data.Id = item.Id;
                    data.CompanyName = item.CompanyName;
                    data.State = item.State;
                    data.Address = item.Address;
                    if (item.RemoveImage)
                    {
                        data.Logo = null;
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
                                data.Logo = memoryStream.ToArray();
                            }
                            else
                            {
                                return View(item);
                            }
                        }
                    }


                    _context.Update(data);
                    await _context.SaveChangesAsync();

                    

                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

    }
}
