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
    public class LinksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppSettings app;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _appEnvironment;
        private readonly LocalizationService _localizationService;
        private readonly IMyLog _MyLog;

        public LinksController(IMyLog MyLog, LocalizationService localizationService, IAppSettings _app, ApplicationDbContext context, UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Hosting.IHostingEnvironment appEnvironment)
        {
            app = _app;
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
            _localizationService = localizationService;
            _MyLog = MyLog;
        }

       
     


        public IActionResult Index(string id)
        {
            //Service ID = id

            string CoId = "";
            string DeId = "";
            string SecId = "";

            if (id != null && id != "")
            {
                SecId = _context.Services.Where(u => u.Id.ToString() == id).Select(u => u.SectionId).FirstOrDefault().ToString();
            }

            if (SecId != null && SecId != "" )
            {
                DeId = _context.Sections.Where(u => u.Id.ToString() == SecId).Select(u => u.DeptId).FirstOrDefault().ToString();
            }

            if (DeId != null && DeId != "")
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == DeId).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }

            ViewData["CompanyId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName",CoId);
            ViewData["DeptId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", DeId);
            ViewData["SectionId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId.ToString() == DeId), "Id", "SectionName", SecId);
            ViewData["ServiceId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId.ToString() == SecId), "Id", "ServiceName", id);

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
                IQueryable<Link> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<LinkViewModel> LinkViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = LinkViewModels });

            }
            catch (Exception ex)
            {
                throw;
            }


        }

        private List<LinkViewModel> GetViewModel(IQueryable<Link> Links)
        {
            List<LinkViewModel> LinkViewModels = new List<LinkViewModel>();

            foreach (Link item in Links)
            {
                LinkViewModel curitem = new LinkViewModel();
                curitem.Id = item.Id;
                curitem.CompanyName = item.Service.Section.Dept.Company.CompanyName;
                curitem.DeptName = item.Service.Section.Dept.DeptName;
                curitem.SectionName = item.Service.Section.SectionName;
                curitem.ServiceName= item.Service.ServiceName;
                curitem.LinkName = item.LinkName;               
                curitem.LinkUserName= item.LinkUserName;              
                curitem.State = item.State;
                LinkViewModels.Add(curitem);
            }
            return LinkViewModels;
        }

        private IQueryable<Link> GetData(JqueryDataTablesParameters param)
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
                string SerFilter = AdditionalValues[4].ToString();


                // getting all Customer data  
                IQueryable<Link> data = Enumerable.Empty<Link>().AsQueryable();

                data = _context.Links                    
                    .Include(r => r.Service)
                    .Include(r => r.Service.Section)
                    .Include(r => r.Service.Section.Dept)
                    .Include(r => r.Service.Section.Dept.Company);

                if (SerFilter != "All")
                {
                    data = data.Where(u => u.ServiceId.ToString() == SerFilter); ;
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
                            sortColumn = "Service.Section.Dept.Company.CompanyName";
                            break;
                        case "DeptName":
                            sortColumn = "Service.Section.Dept.DeptName";
                            break;
                        case "SectionName":
                            sortColumn = "Service.Section.SectionName";
                            break;
                        case "ServiceName":
                            sortColumn = "Service.ServiceName";
                            break;
                        case "LinkManagerName":
                            sortColumn = "LinkManager.PersonName";
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
                    data = data.Where(m => m.Service.Section.Dept.Company.CompanyName.ToLower().Contains(searchValue.ToLower())
                    || m.Service.Section.Dept.DeptName.ToLower().Contains(searchValue.ToLower())
                    || m.Service.Section.SectionName.ToLower().Contains(searchValue.ToLower())
                    || m.Service.ServiceName.ToLower().Contains(searchValue.ToLower())
                    || m.LinkName.ToLower().Contains(searchValue.ToLower())
                    || m.LinkUserName.ToLower().Contains(searchValue.ToLower())                    
                    //|| m.Description.ToString().ToLower().Contains(searchValue.ToLower())                   
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

            List<LinkViewModel> LinkViewModels = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            //Returning Json Data  
            return Json(new { data = LinkViewModels });

        }


        public async Task<string> Delete(Guid Id)
        {
            var data = _context.Links.Where(u => u.Id == Id).FirstOrDefault();

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

        // GET: Links/Details/5        
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Links
                .Include(r => r.Service)
                .Include(r => r.Web)
                .Include(r => r.LinkPermissions)
                .Include(r => r.Service.Section)
                .Include(r => r.Service.Section.Dept)
                .Include(r => r.Service.Section.Dept.Company)               
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            LinkViewModel curitem = new LinkViewModel();
            curitem.Id = item.Id;
            curitem.ServiceId = item.Service.Id;

            curitem.CompanyName = item.Service.Section.Dept.Company.CompanyName;
            curitem.DeptName = item.Service.Section.Dept.DeptName;
            curitem.SectionName = item.Service.Section.SectionName;
            curitem.ServiceName = item.Service.ServiceName;
            curitem.LinkName = item.LinkName;            
            curitem.LinkUserName = item.LinkUserName;            
            curitem.LinkUserPassword = item.LinkUserPassword;           
            curitem.WebName = item.Web.WebName;
            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;          
            curitem.State = item.State;
            curitem.AuthorizeUsers = _context.Set<ApplicationUser>().Where(u => u.IsActive == true && item.LinkPermissions.Select(r=>r.PersonId).Contains(u.Id)).Select(u=>u.PersonName).ToList();
            return View(curitem);
        }

        // GET: Links/Create        
        public IActionResult Create(string id)
        {
            string CoId = "";
            string DeId = "";
            string SecId = "";

            if (id != null && id != "")
            {
                SecId = _context.Services.Where(u => u.Id.ToString() == id).Select(u => u.SectionId).FirstOrDefault().ToString();
            }

            if (SecId != null && SecId != "")
            {
                DeId = _context.Sections.Where(u => u.Id.ToString() == SecId).Select(u => u.DeptId).FirstOrDefault().ToString();
            }

            if (DeId != null && DeId != "")
            {
                CoId = _context.Depts.Where(u => u.Id.ToString() == DeId).Select(u => u.CompanyId).FirstOrDefault().ToString();
            }

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", CoId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId.ToString() == CoId), "Id", "DeptName", DeId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId.ToString() == DeId), "Id", "SectionName", SecId);
            ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId.ToString() == SecId), "Id", "ServiceName", id);

            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName");
            ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName");

            return View();
        }

        // POST: Links/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]                
        public async Task<IActionResult> Create(LinkViewModel item)
        {

            if (ModelState.IsValid)
            {
                Link curitem = new Link();
                curitem.Id = new Guid();                
                curitem.ServiceId = item.ServiceId;                
                curitem.LinkName = item.LinkName;
                curitem.LinkUserName = item.LinkUserName;
                curitem.LinkUserPassword = item.LinkUserPassword;
                curitem.WebId = item.WebId;
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
                            ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId == item.SectionId), "Id", "ServiceName", item.ServiceId);

                            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName",item.AuthorizeUsers);
                            ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName",item.WebId);

                            return View(item);

                        }
                    }
                }

                _context.Add(curitem);
                await _context.SaveChangesAsync();

                // Set Permission to the links
                SetPermissions(curitem.Id, item.AuthorizeUsers);
                
                return RedirectToAction(nameof(Index), new { id = curitem.ServiceId.ToString() });
            }



            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", item.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == item.CompanyId), "Id", "DeptName", item.DeptId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == item.DeptId), "Id", "SectionName", item.SectionId);
            ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId == item.SectionId), "Id", "ServiceName", item.ServiceId);
            ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName", item.WebId);

            return View(item);
        }

        private bool SetPermissions(Guid id, IEnumerable<string> AuthorizeUsers)
        {
            try
            {
                List<LinkPermission> OldPermissions = _context.LinkPermissions.Where(u => u.LinkId == id).ToList();
                if (OldPermissions.Any())
                {
                    _context.LinkPermissions.RemoveRange(OldPermissions);
                    _context.SaveChanges();
                }

                List<LinkPermission> NewPermissions = new List<LinkPermission>();

                
                foreach (string user in AuthorizeUsers)
                {
                    NewPermissions.Add(new LinkPermission { LinkId = id, PersonId = user });
                }

                _context.LinkPermissions.AddRange(NewPermissions);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", _localizationService.GetLocalized("There is an error in setting Permissions.") + ex.Message);
                return false;
            }
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
        public List<CustomListItem> GetServiceList(Guid Id)
        {
            List<CustomListItem> SerList = new List<CustomListItem>();

            var item = _context.Services
                .Include(u => u.Section)
                .Where(u => u.Section.Id == Id && u.State == Models.Shared.State.Active);

            if (item != null)
            {
                foreach (var Sec in item)
                {
                    CustomListItem ListItem = new CustomListItem
                    {
                        Id = Sec.Id,
                        Name = Sec.ServiceName
                    };

                    SerList.Add(ListItem);
                }
            }
            return SerList;
        }
        // GET: Links/Edit/5
        public IActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = _context.Links
                .Include(r => r.Service)
                .Include(r => r.Service.Section)
                .Include(r => r.Service.Section.Dept)
                .Include(r => r.Service.Section.Dept.Company)
                .Include(r => r.LinkPermissions)
                 .Where(u => u.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            LinkViewModel curitem = new LinkViewModel();
            curitem.Id = item.Id;
            curitem.CompanyId = item.Service.Section.Dept.Company.Id;
            curitem.CompanyName= item.Service.Section.Dept.Company.CompanyName;
            curitem.DeptId = item.Service.Section.Dept.Id;
            curitem.DeptName= item.Service.Section.Dept.DeptName;
            curitem.SectionId = item.Service.Section.Id;
            curitem.SectionName = item.Service.Section.SectionName;
            curitem.ServiceId = item.Service.Id;
            curitem.ServiceName = item.Service.ServiceName;

            curitem.LinkName = item.LinkName;
            curitem.LinkUserName = item.LinkUserName;
            curitem.LinkUserPassword = item.LinkUserPassword;
            curitem.WebId = item.WebId;

            curitem.ExistingLogo = item.Logo;
            curitem.Description = item.Description;
            curitem.State = item.State;

            curitem.AuthorizeUsers = item.LinkPermissions.Select(r => r.PersonId).ToList();

            ViewData["CId"] = new SelectList(_context.Set<Company>().Where(u => u.State == Models.Shared.State.Active), "Id", "CompanyName", curitem.CompanyId);
            ViewData["DId"] = new SelectList(_context.Set<Dept>().Where(u => u.State == Models.Shared.State.Active && u.CompanyId == curitem.CompanyId), "Id", "DeptName", curitem.DeptId);
            ViewData["SecId"] = new SelectList(_context.Set<Section>().Where(u => u.State == Models.Shared.State.Active && u.DeptId == curitem.DeptId), "Id", "SectionName", curitem.SectionId);
            ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId == curitem.SectionId), "Id", "ServiceName", curitem.ServiceId);

            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName", curitem.AuthorizeUsers);

            ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName", curitem.WebId);

            return View(curitem);
        }


        // POST: Links/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(LinkViewModel item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    // To remove Old SalesMan from Saleman role
                    Link data = _context.Links.Where(u => u.Id == item.Id).FirstOrDefault();             

                    data.Id = item.Id;
                    data.ServiceId = item.ServiceId;
                    data.LinkName = item.LinkName;
                    data.LinkUserName = item.LinkUserName;
                    data.LinkUserPassword = item.LinkUserPassword;
                    data.WebId = item.WebId;
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
                                ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId == item.SectionId), "Id", "ServiceName", item.ServiceId);
                                ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName", item.AuthorizeUsers);
                                ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName", item.WebId);

                                return View(item);
                            }
                        }
                    }
                    _context.Update(data);
                    await _context.SaveChangesAsync();

                    // Set Permission to the links
                    SetPermissions(data.Id, item.AuthorizeUsers);


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinkExists(item.Id))
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
            ViewData["SerId"] = new SelectList(_context.Set<Service>().Where(u => u.State == Models.Shared.State.Active && u.SectionId == item.SectionId), "Id", "ServiceName", item.ServiceId);
            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName", item.AuthorizeUsers);
            ViewData["WebList"] = new SelectList(_context.Set<Web>(), "WebName", "WebName", item.WebId);

            return View(item);
        }

        private bool LinkExists(Guid id)
        {
            return _context.Links.Any(e => e.Id == id);
        }

    }
}
