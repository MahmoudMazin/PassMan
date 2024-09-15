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
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppSettings app;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _appEnvironment;
        private readonly LocalizationService _localizationService;


        public SectionsController(LocalizationService localizationService, IAppSettings _app, ApplicationDbContext context, UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Hosting.IHostingEnvironment appEnvironment)
        {
            app = _app;
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
            _localizationService = localizationService;
        }

        // GET: Sections
       
        public IActionResult Index(string id)
        {
            string CoId = "";
            if (id != null && id != "" )
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == id).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName",CoId);
            ViewData["DeptId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", id);
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
                IQueryable<Section> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<SectionViewModel> SectionViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = SectionViewModels });

            }
            catch (Exception)
            {
                throw;
            }


        }

        private List<SectionViewModel> GetViewModel(IQueryable<Section> Sections)
        {
            List<SectionViewModel> SectionViewModels = new List<SectionViewModel>();

            foreach (Section item in Sections)
            {
                SectionViewModel curitem = new SectionViewModel();
                curitem.Id = item.Id;
                curitem.CompanyName = item.Dept.Company.CompanyName;
                curitem.DeptName = item.Dept.DeptName;
                curitem.SectionName = item.SectionName;
                curitem.Description= item.Description;
                curitem.Services = item.Services.Where(u=>u.State == Models.Shared.State.Active ).Count();                
                curitem.State = item.State;
                SectionViewModels.Add(curitem);
            }
            return SectionViewModels;
        }

        private IQueryable<Section> GetData(JqueryDataTablesParameters param)
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
                string DeptFilter = AdditionalValues[2].ToString();
                
                // getting all Customer data  
                IQueryable<Section> data = Enumerable.Empty<Section>().AsQueryable();

                data = _context.Sections
                    .Include(r => r.Services)
                    .Include(r => r.Dept)
                    .ThenInclude(r => r.Company);
                    
                if (DeptFilter != "All")
                {
                    data = data.Where(u => u.DeptId.ToString() == DeptFilter); ;
                }

                //State  
                if (!string.IsNullOrEmpty(StateFiler))
                {
                    if (StateFiler == "true")
                        data = data.Where(m => m.State == Models.Shared.State.Active);
                }

                //Default Sorting
                //data = data.OrderBy("ScheduledDate");

                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {

                    sortColumn = sortColumn.Replace(" DESC", "");
                    switch (sortColumn)
                    {
                        case "CompanyName":
                            sortColumn = "Dept.Company.CompanyName";
                            break;
                        case "DeptName":
                            sortColumn = "Dept.DeptName";
                            break;
                        default:
                            sortColumn = "Id";
                            break;
                    }
                    sortColumn += " " + sortColumnDirection.ToString();

                    data = data.OrderBy(sortColumn);
                }

                


                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.Dept.Company.CompanyName.ToLower().Contains(searchValue.ToLower())
                    || m.Dept.DeptName.ToLower().Contains(searchValue.ToLower()) 
                    || m.SectionName.ToLower().Contains(searchValue.ToLower())
                    || (m.Description != null && m.Description.ToString().ToLower().Contains(searchValue.ToLower()))
                    );
                }
                return data;
            }
            catch (Exception ex)
            {
                string e = ex.Message.ToString();
                throw;
            }
        }

                
        public IActionResult GetExcel()
        {
            var param = HttpContext.Session.GetString(nameof(JqueryDataTablesParameters));

            List<SectionViewModel> SectionViewModels = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            //Returning Json Data  
            return Json(new { data = SectionViewModels });

        }


        public async Task<string> Delete(Guid Id)
        {
            var data = _context.Sections.Include(u => u.Services).Where(u => u.Id == Id).FirstOrDefault();

            var childcounts = data.Services.Where(u => u.State == Models.Shared.State.Active).Count();

            // here we need to write logic for checking all related record before Inactive.
            if (data.State == Models.Shared.State.Active && childcounts > 0)
            {
                return string.Format(_localizationService.GetLocalized("Unable to Inactive this Section as {0} Services are active in this Section. Please inactivate all Services and then try again."), childcounts );
            }

            data.State = data.State == Models.Shared.State.Active ? Models.Shared.State.InActive : Models.Shared.State.Active;
            try
            {
                _context.Update(data);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return _localizationService.GetLocalized("Error:") + ex.Message;
            }

            return "Succeeded";
        }

        // GET: Sections/Details/5        
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Sections
                .Include(r => r.Dept)
                .Include(r => r.Dept.Company)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            SectionViewModel curitem = new SectionViewModel();
            curitem.Id = item.Id;
            curitem.DeptId = item.DeptId;
            curitem.CompanyName = item.Dept.Company.CompanyName;
            curitem.DeptName = item.Dept.DeptName;
            curitem.SectionName = item.SectionName;
            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;
            curitem.State = item.State;

            return View(curitem);
        }

        // GET: Sections/Create        
        public IActionResult Create(string id)
        {
            string CoId = "";
            if (id != null && id != "" )
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == id).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", CoId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", id);
    
            return View();
        }

        // POST: Sections/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]                
        public async Task<IActionResult> Create(SectionViewModel item)
        {
            if (ModelState.IsValid)
            {
                Section curitem = new Section();
                curitem.Id = new Guid();

                
                curitem.DeptId = item.DeptId;
                curitem.SectionName = item.SectionName;
                curitem.Description = item.Description;
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

                            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName",item.CompanyId);
                            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
         
                            return View(item);
                        }
                    }
                }

                _context.Add(curitem);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = curitem.DeptId.ToString() });
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
       
            return View(item);
        }

        public List<CustomListItem> GetDeptList(Guid Id)
        {
            List<CustomListItem> DeptList = new List<CustomListItem>();

            var item = _context.Depts
                .Include(u=>u.Company)
                .Where(u => u.Company.Id == Id && u.State == Models.Shared.State.Active);

            if (item != null)
            {
                foreach (var dept in item)
                {
                    CustomListItem ListItem = new CustomListItem
                    {
                        Id = dept.Id,
                        Name = dept.DeptName
                    };

                    DeptList.Add(ListItem);
                }
            }
            return DeptList;
        }



        // GET: Sections/Edit/5
        public IActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = _context.Sections
                .Include(r => r.Dept)
                .Include(r => r.Dept.Company)
                .Where(u => u.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            SectionViewModel curitem = new SectionViewModel();
            curitem.Id = item.Id;
            curitem.CompanyId = item.Dept.Company.Id;
            curitem.CompanyName= item.Dept.Company.CompanyName;
            curitem.DeptId = item.Dept.Id;
            curitem.DeptName= item.Dept.DeptName;
            curitem.SectionName = item.SectionName;
            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;
            curitem.State = item.State;


            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", curitem.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == curitem.CompanyId), "Id", "DeptName", curitem.DeptId);
     
            return View(curitem);
        }


        // POST: Sections/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(SectionViewModel item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    // To remove Old SalesMan from Saleman role
                    Section data = _context.Sections.Where(u => u.Id == item.Id).FirstOrDefault();             

                    data.Id = item.Id;
                    data.DeptId = item.DeptId;
                    data.SectionName = item.SectionName;
                    data.Description = item.Description;
                     data.State = item.State;
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
                                ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
                                ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
                                return View(item);
                            }
                        }
                    }
                    _context.Update(data);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = item.DeptId.ToString() });
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
            return View(item);
        }

        private bool SectionExists(Guid id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }
    }
}
