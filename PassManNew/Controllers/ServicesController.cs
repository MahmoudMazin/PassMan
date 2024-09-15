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
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Hosting;
using PassManNew.Resources;

namespace PassManNew.Controllers
{

    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppSettings app;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _appEnvironment;
        private readonly LocalizationService _localizationService;

        public ServicesController(LocalizationService localizationService, IAppSettings _app, ApplicationDbContext context, UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Hosting.IHostingEnvironment appEnvironment)
        {
            app = _app;
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
            _localizationService = localizationService;
        }

        // GET: Services
       
        public IActionResult Index(string id)
        {
            string CoId = "";
            string DeId = ""; 

            if (id != null && id != "" )
            {
                DeId = _context.Sections.Where(u => u.Id.ToString() == id).Select(u => u.DeptId).FirstOrDefault().ToString();
            }

            if (DeId != null && DeId != "")
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == DeId).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }

            ViewData["CompanyId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName",CoId);
            ViewData["DeptId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", DeId);
            ViewData["SectionId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId.ToString() == DeId), "Id", "SectionName", id);

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
                IQueryable<Service> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<ServiceViewModel> ServiceViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = ServiceViewModels });

            }
            catch (Exception)
            {
                throw;
            }


        }

        private List<ServiceViewModel> GetViewModel(IQueryable<Service> Services)
        {
            List<ServiceViewModel> ServiceViewModels = new List<ServiceViewModel>();

            foreach (Service item in Services)
            {
                ServiceViewModel curitem = new ServiceViewModel();
                curitem.Id = item.Id;
                curitem.CompanyName = item.Section.Dept.Company.CompanyName;
                curitem.DeptName = item.Section.Dept.DeptName;
                curitem.SectionName = item.Section.SectionName;
                curitem.ServiceName = item.ServiceName;
                curitem.Description= item.Description;
                curitem.Links = item.Links.Where(u=>u.State == Models.Shared.State.Active ).Count();                
                curitem.State = item.State;
                ServiceViewModels.Add(curitem);
            }
            return ServiceViewModels;
        }

        private IQueryable<Service> GetData(JqueryDataTablesParameters param)
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
                string SecFilter = AdditionalValues[3].ToString();

                // getting all Customer data  
                IQueryable<Service> data = Enumerable.Empty<Service>().AsQueryable();

                data = _context.Services
                    .Include(r => r.Links)
                    .Include(r => r.Section)
                    .ThenInclude(r=>r.Dept)
                    .ThenInclude(r => r.Company);

                if (SecFilter != "All")
                {
                    data = data.Where(u => u.SectionId.ToString() == SecFilter); ;
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
                            sortColumn = "Section.Dept.Company.CompanyName";
                            break;
                        case "DeptName":
                            sortColumn = "Section.Dept.DeptName";
                            break;
                        case "ServiceName":
                            sortColumn = "Section.SectionName";
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
                    data = data.Where(m => m.Section.Dept.Company.CompanyName.ToLower().Contains(searchValue.ToLower())
                    || m.Section.Dept.DeptName.ToLower().Contains(searchValue.ToLower())
                    || m.Section.SectionName.ToLower().Contains(searchValue.ToLower())
                    || m.ServiceName.ToLower().Contains(searchValue.ToLower())
                    || (m.Description != null && m.Description.ToString().ToLower().Contains(searchValue.ToLower()))
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

            List<ServiceViewModel> ServiceViewModels = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            //Returning Json Data  
            return Json(new { data = ServiceViewModels });

        }


        public async Task<string> Delete(Guid Id)
        {
            var data = _context.Services.Include(u => u.Section).Where(u => u.Id == Id).FirstOrDefault();

            var childcounts = data.Links.Where(u => u.State == Models.Shared.State.Active).Count();

            // here we need to write logic for checking all related record before Inactive.
            if (data.State == Models.Shared.State.Active && childcounts > 0)
            {
                return string.Format(_localizationService.GetLocalized("Unable to Inactive this Service as {0} Links are active in this Service. Please inactivate all Links and then try again."),childcounts);
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

        // GET: Services/Details/5        
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Services
                .Include(r => r.Section)
                .Include(r => r.Section.Dept)
                .Include(r => r.Section.Dept.Company)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            ServiceViewModel curitem = new ServiceViewModel();
            curitem.Id = item.Id;
            curitem.SectionId = item.SectionId;
            curitem.CompanyName = item.Section.Dept.Company.CompanyName;
            curitem.DeptName = item.Section.Dept.DeptName;
            curitem.SectionName = item.Section.SectionName;
            curitem.ServiceName = item.ServiceName;
            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;
            curitem.State = item.State;

            return View(curitem);
        }

        // GET: Services/Create        
        public IActionResult Create(string id)
        {
            
            string CoId = "";
            string DeId = "";

            if (id != null && id != "")
            {
                DeId = _context.Sections.Where(u => u.Id.ToString() == id).Select(u => u.DeptId).FirstOrDefault().ToString();
            }

            if (DeId != null && DeId != "")
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == DeId).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", CoId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", DeId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId.ToString() == DeId), "Id", "SectionName", id);
                     
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]                
        public async Task<IActionResult> Create(ServiceViewModel item)
        {

            if (ModelState.IsValid)
            {
                Service curitem = new Service();
                curitem.Id = new Guid();

                
                curitem.SectionId = item.SectionId;
                curitem.ServiceName = item.ServiceName;
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

                                                        
                            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
                            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
                            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == item.DeptId), "Id", "SectionName", item.SectionId);

                            return View(item);

                        }
                    }
                }

                _context.Add(curitem);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = curitem.SectionId.ToString() });
            }



            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == item.DeptId), "Id", "SectionName", item.SectionId);

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

        public List<CustomListItem> GetSectionList(Guid Id)
        {
            List<CustomListItem> SectionList = new List<CustomListItem>();

            var item = _context.Sections
                .Include(u => u.Dept)
                .Where(u => u.Dept.Id == Id && u.State == Models.Shared.State.Active);

            if (item != null)
            {
                foreach (var Sec in item)
                {
                    CustomListItem ListItem = new CustomListItem
                    {
                        Id = Sec.Id,
                        Name = Sec.SectionName
                    };

                    SectionList.Add(ListItem);
                }
            }
            return SectionList;
        }

        // GET: Services/Edit/5
        public IActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = _context.Services
                .Include(r => r.Section)
                .Include(r => r.Section.Dept)
                .Include(r => r.Section.Dept.Company)
                .Where(u => u.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            ServiceViewModel curitem = new ServiceViewModel();
            curitem.Id = item.Id;
            curitem.CompanyId = item.Section.Dept.Company.Id;
            curitem.CompanyName= item.Section.Dept.Company.CompanyName;
            curitem.DeptId = item.Section.Dept.Id;
            curitem.DeptName= item.Section.Dept.DeptName;
            curitem.SectionId = item.Section.Id;
            curitem.SectionName = item.Section.SectionName;
            curitem.ServiceName = item.ServiceName;
            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;
           curitem.State = item.State;


            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", curitem.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == curitem.CompanyId), "Id", "DeptName", curitem.DeptId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId== curitem.DeptId), "Id", "SectionName", curitem.SectionId);
      
            return View(curitem);
        }


        // POST: Services/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(ServiceViewModel item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    // To remove Old SalesMan from Saleman role
                    Service data = _context.Services.Where(u => u.Id == item.Id).FirstOrDefault();             

                    data.Id = item.Id;
                    data.SectionId = item.SectionId;
                    data.ServiceName = item.ServiceName;
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
                                ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == item.DeptId), "Id", "SectionName", item.SectionId);
                                return View(item);
                            }
                        }
                    }
                    _context.Update(data);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = item.SectionId.ToString() });
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == item.DeptId), "Id", "SectionName", item.SectionId);
            return View(item);
        }

        private bool ServiceExists(Guid id)
        {
            return _context.Services.Any(e => e.Id == id);
        }

    }
}
