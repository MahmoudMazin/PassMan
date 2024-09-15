using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JqueryDataTables.ServerSide.AspNetCoreWeb.ActionResults;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PassManNew.Data;
using PassManNew.Models;
using PassManNew.Models.Shared;
using PassManNew.Resources;
using PassManNew.Services.EmailService;

namespace PassManNew.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;       
        private readonly ApplicationDbContext _context;
        private readonly IAppSettings app;      
        private readonly int WeekNo;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly LocalizationService _localizationService;
        private readonly IEmailService _emailSender;
        private readonly IMyLog _MyLog;

        public HomeController(IMyLog MyLog, IEmailService emailSender, LocalizationService localizationService, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment,IAppSettings _app,ApplicationDbContext context, UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)
        {
            _emailSender = emailSender;
            _localizationService = localizationService;
            app = _app;
            _userManager = userManager;
            _signInManager = signInManager;           
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            WeekNo = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(System.DateTime.Now.Date, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
            _MyLog = MyLog;
        }

        public JsonResult GetRPAJson(Guid linkId)
        {

            if (linkId == null)
            {
                return null;
            }

            var item = _context.Links.Include(u => u.Web)
                .Include(u=>u.Service)
                .ThenInclude(u=>u.Section)
                .ThenInclude(u => u.Dept)
                .ThenInclude(u => u.Company)
                .FirstOrDefault(m => m.Id == linkId);
       
            if (item == null)
            {
                return null;
            }

            RPAJsonModel rpaJsonModel = JsonConvert.DeserializeObject<RPAJsonModel>(item.Web.JsonRPA);

            if (item.Web.UserNameField != "")
            {
                rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == item.Web.UserNameField).First().Value = item.LinkUserName;

            }
            if (item.Web.PwdNameField != "")
            {
                rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == item.Web.PwdNameField).First().Value = item.LinkUserPassword;

            }

            //switch (rpaJsonModel.Name)
            //{
            //    case "PassMan-RPA-Qiwa":
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=login_username").First().Value = item.LinkUserName;
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=login_password").First().Value = item.LinkUserPassword;
            //        break;
            //    case "PassMan-RPA-IAMGOV":
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=j_username").First().Value = item.LinkUserName;
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=j_password").First().Value = item.LinkUserPassword;
            //        break;
            //    case "PassMan-RPA-TAMM":
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=mainForm:username").First().Value = item.LinkUserName;
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=mainForm:password").First().Value = item.LinkUserPassword;
            //        break;
            //    case "PassMan-RPA-Waie.Aldress":
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=MainContent_ucLogin_txtUserName").First().Value = item.LinkUserName;
            //        rpaJsonModel.Commands.Where(u => u.Command == "type" && u.Target == "id=MainContent_ucLogin_txtPass").First().Value = item.LinkUserPassword;
            //        break;
            //    default:

            //        break;
            //}

            _MyLog.CreateLog(Models.Shared.LogType.AccessWeb, $"{item.LinkName} | {item.Service.ServiceName} | {item.Service.Section.SectionName} | {item.Service.Section.Dept.DeptName} | {item.Service.Section.Dept.Company.CompanyName}", User.Identity.Name);

            return Json(rpaJsonModel);
        }


        public IActionResult Index()
        {
            
            return View(GetDashBoardModel());
        }

        public PartialViewResult GetNavigationBlock(string category, string Id, string RetId)
        {
            //_context
                        
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();
           
                CategoryNavViewModel item = new CategoryNavViewModel();                
                switch (category)
                {
                    case "1":
                        model = GetCompanies(RetId); 
                        break;
                    case "2":
                        model = GetDepts(Id, RetId);
                        break;
                    case "3":
                        model = GetSections(Id,RetId);
                        break;
                    case "4":
                        model = GetServices(Id,RetId);
                        break;
                    case "5":
                        model = GetLinks(Id,RetId);
                        break;
                    default:
                        break;
                }
            

            return PartialView("_NavigationBlock",model);
        }

        private List<CategoryNavViewModel> GetCompanies(string RetId)
        {
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();


            //Filter Records as per Roles        
            List<Company> companies = _context.Companies.FromSqlRaw<Company>("Select Distinct c.* from Company c " +
                                        "inner join dept d on c.id = d.CompanyId " +
                                        "inner join section s on d.id = s.DeptId " +
                                        "inner join service se on s.id = se.SectionId " +
                                        "inner join link l on se.id = l.ServiceId " +
                                        "inner join LinkPermission p on l.id = p.LinkId " +
                                        "where p.PersonId = {0}", _userManager.GetUserId(User))
                                        .ToList();

            if (User.IsInRole("Admin"))
            {
                companies = _context.Companies.FromSqlRaw<Company>("Select Distinct c.* from Company c " +
                                            "inner join dept d on c.id = d.CompanyId " +
                                            "inner join section s on d.id = s.DeptId " +
                                            "inner join service se on s.id = se.SectionId " +
                                            "inner join link l on se.id = l.ServiceId ")
                                            .ToList();
            }


            foreach (Company item in companies)
            {
                CategoryNavViewModel data = new CategoryNavViewModel();
                data.Category = _localizationService.GetLocalized("Companies");
                data.Breadcumb = "";
                data.TabTitle = _localizationService.GetLocalized("Company"); 
                data.TabColor = "bg-amber";
                data.TabIcon = "business";
                data.Id = item.Id;
                data.Logo = item.Logo;
                data.Name = item.CompanyName;
                data.NextNav = String.Format("NavigateNext('2','{0}');", item.Id);               
                model.Add(data);
            }

            return model;
        }


        private List<CategoryNavViewModel> GetDepts(string Id,string RetId)
        {
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();


            //Filter Records as per Roles        
            List<Dept> Depts = _context.Depts.FromSqlRaw<Dept>("Select Distinct d.* from Dept d " +                                        
                                        "inner join section s on d.id = s.DeptId " +
                                        "inner join service se on s.id = se.SectionId " +
                                        "inner join link l on se.id = l.ServiceId " +
                                        "inner join LinkPermission p on l.id = p.LinkId " +
                                        "where p.PersonId = {0} and d.CompanyId = {1}", _userManager.GetUserId(User), Id)
                                        .Include(u => u.Company)
                                        .ToList();

            if (User.IsInRole("Admin"))
            {
                Depts = _context.Depts.FromSqlRaw<Dept>("Select Distinct d.* from Dept d " +
                                        "inner join section s on d.id = s.DeptId " +
                                        "inner join service se on s.id = se.SectionId " +
                                        "inner join link l on se.id = l.ServiceId " +                                      
                                        "where d.CompanyId = {0}",Id)
                                        .Include(u=>u.Company)
                                        .ToList();
            }


            foreach (Dept item in Depts)
            {
                CategoryNavViewModel data = new CategoryNavViewModel();
                data.Category = _localizationService.GetLocalized("Departments");
                data.Breadcumb = String.Format("<a onclick =NavigateNext('2','{0}') > {1} </a> >",Id, item.Company.CompanyName);
                data.TabTitle = _localizationService.GetLocalized("Department");
                data.TabColor = "bg-lime";
                data.TabIcon = "apps";
                data.Id = item.Id;
                data.Logo = item.Logo;
                data.Name = item.DeptName;
                data.NextNav = String.Format("NavigateNext('3','{0}')", item.Id);
                model.Add(data);
            }

            return model;
        }

        private List<CategoryNavViewModel> GetSections(string Id, string RetId)
        {
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();


            //Filter Records as per Roles        
            List<Section> sections = _context.Sections.FromSqlRaw<Section>("Select Distinct s.* from Section s " +
                                        "inner join service se on s.id = se.SectionId " +
                                        "inner join link l on se.id = l.ServiceId " +
                                        "inner join LinkPermission p on l.id = p.LinkId " +
                                        "where p.PersonId = {0} and s.DeptId = {1}", _userManager.GetUserId(User), Id)
                                        .Include(u => u.Dept.Company)
                                        .Include(u => u.Dept)
                                        .ToList();

            if (User.IsInRole("Admin"))
            {
                sections = _context.Sections.FromSqlRaw<Section>("Select Distinct s.* from Section s " +
                                        "inner join service se on s.id = se.SectionId " +
                                        "inner join link l on se.id = l.ServiceId " +                                       
                                        "where s.DeptId = {0}", Id)
                                        .Include(u => u.Dept.Company)
                                        .Include(u => u.Dept)
                                        .ToList();
            }


            foreach (Section item in sections)
            {
                CategoryNavViewModel data = new CategoryNavViewModel();
                data.Category = _localizationService.GetLocalized("Sections");
                data.Breadcumb = String.Format("<a onclick =NavigateNext('2','{0}') > {1} </a> > ", item.Dept.Company.Id, item.Dept.Company.CompanyName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('3','{0}')> {1} </a> > ", Id, item.Dept.DeptName );
                data.TabTitle = _localizationService.GetLocalized("Section");
                data.TabColor = "bg-light-green";
                data.TabIcon = "fullscreen";
                data.Id = item.Id;
                data.Logo = item.Logo;
                data.Name = item.SectionName;
                data.NextNav = String.Format("NavigateNext('4','{0}')", item.Id); 
                model.Add(data);
            }

            return model;
        }

        private List<CategoryNavViewModel> GetServices(string Id, string RetId)
        {
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();


            //Filter Records as per Roles        
            List<Service> services = _context.Services.FromSqlRaw<Service>("Select Distinct se.* from Service se " +
                                        "inner join link l on se.id = l.ServiceId " +
                                        "inner join LinkPermission p on l.id = p.LinkId " +
                                        "where p.PersonId = {0} and se.SectionId = {1}", _userManager.GetUserId(User), Id)
                                        .Include(u => u.Section.Dept.Company)
                                        .Include(u => u.Section.Dept)
                                        .Include(u => u.Section)
                                        .ToList();

            if (User.IsInRole("Admin"))
            {
                services = _context.Services.FromSqlRaw<Service>("Select Distinct se.* from Service se " +
                                        "inner join link l on se.id = l.ServiceId " +                                       
                                        "where se.SectionId = {0}", Id)
                                        .Include(u => u.Section.Dept.Company)
                                        .Include(u => u.Section.Dept)
                                        .Include(u => u.Section)
                                        .ToList();
            }


            foreach (Service item in services)
            {
                CategoryNavViewModel data = new CategoryNavViewModel();
                data.Category = _localizationService.GetLocalized("Services"); 
                data.Breadcumb = String.Format("<a onclick =NavigateNext('2','{0}') > {1} </a> > ", item.Section.Dept.Company.Id, item.Section.Dept.Company.CompanyName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('3','{0}')> {1} </a> > ", item.Section.Dept.Id, item.Section.Dept.DeptName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('4','{0}')> {1} </a> > ", Id, item.Section.SectionName);
                data.TabTitle = _localizationService.GetLocalized("Service"); 
                data.TabColor = "bg-green";
                data.TabIcon = "language";
                data.Id = item.Id;
                data.Logo = item.Logo;
                data.Name = item.ServiceName;
                data.NextNav = String.Format("NavigateNext('5','{0}')", item.Id); 
                model.Add(data);
            }

            return model;
        }
        private List<CategoryNavViewModel> GetLinks(string Id, string RetId)
        {
            List<CategoryNavViewModel> model = new List<CategoryNavViewModel>();


            //Filter Records as per Roles        
            List<Link> links = _context.Links.FromSqlRaw<Link>("Select Distinct l.* from link l " +
                                        "inner join LinkPermission p on l.id = p.LinkId " +
                                        "where p.PersonId = {0} and l.ServiceId= {1} and l.State = 1", _userManager.GetUserId(User), Id)
                                        .Include(u => u.Service.Section.Dept.Company)
                                        .Include(u => u.Service.Section.Dept)
                                        .Include(u => u.Service.Section)
                                        .Include(u=> u.Service)
                                        .ToList();

            if (User.IsInRole("Admin"))
            {
                links = _context.Links.FromSqlRaw<Link>("Select Distinct l.* from link l " +
                                        "where l.ServiceId = {0} and l.State = 1", Id)
                                        .Include(u => u.Service.Section.Dept.Company)
                                        .Include(u => u.Service.Section.Dept)
                                        .Include(u => u.Service.Section)
                                        .Include(u => u.Service)
                                        .ToList();
            }


            foreach (Link item in links)
            {
                CategoryNavViewModel data = new CategoryNavViewModel();
                data.Category = _localizationService.GetLocalized("Links");
                
                data.Breadcumb = String.Format("<a onclick =NavigateNext('2','{0}') > {1} </a> > ", item.Service.Section.Dept.Company.Id, item.Service.Section.Dept.Company.CompanyName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('3','{0}')> {1} </a> > ", item.Service.Section.Dept.Id, item.Service.Section.Dept.DeptName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('4','{0}')> {1} </a> > ", item.Service.Section.Id, item.Service.Section.SectionName);
                data.Breadcumb += String.Format("<a onclick =NavigateNext('5','{0}')> {1} </a> > ", Id, item.Service.ServiceName);

                data.TabTitle = _localizationService.GetLocalized("Link"); 
                data.TabColor = "bg-teal";
                data.TabIcon = "web";
                data.Id = item.Id;
                data.Logo = item.Logo;
                data.Name = item.LinkName;
                data.NextNav = String.Format("RunRPA('{0}')", item.Id);                
                model.Add(data);
            }

            return model;
        }
        public Dashboard GetDashBoardModel()
        {
            Dashboard UsrDashBoard = new Dashboard();
            var usr = User;
            var usrid = _userManager.GetUserId(User);

            //UsrDashBoard
            return UsrDashBoard;
        }


        public async Task<ActionResult> GetUserPhoto(string id)
        {
            if (id == null || id == "")
                return File((await _userManager.GetUserAsync(User)).UserPhoto, "Image/Jpeg");
            else
            {
                var usr = await _userManager.FindByIdAsync(id);
                if (usr != null)
                    return File(usr.UserPhoto, "Image/Jpeg");
                else
                    return null;
            }
            
        }

        public JsonResult GetDataTableLang()
        {
            string contentRootPath = _hostingEnvironment.WebRootPath;
            var filepath = "";

            if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                filepath = "/json/datatable-ar.json";
            else
                filepath = "/json/datatable-en.json";
            var JSON = System.IO.File.ReadAllText(contentRootPath + filepath);

            return Json(JsonConvert.DeserializeObject(JSON));
        }
               

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        [Authorize(Roles = "Admin")]
        public IActionResult AppUsers()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowAppUsers([FromBody]JqueryDataTablesParameters param)
        {

            //return GetAppUsersData(HttpContext.Request.Query);
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
                var appUsers = GetAppUsersData(param);

                //total number of rows counts   
                recordsTotal = appUsers.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                appUsers = appUsers.Skip(skip).Take(pageSize);



                //Returning Json Data  
                List<UserViewModel> userViewModels = GetViewModel(appUsers);                

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = userViewModels });

            }
            catch (Exception)
            {
                throw;
            }

            // return View();
        }


        [Authorize(Roles = "Admin")]
        private IQueryable<ApplicationUser> GetAppUsersData(JqueryDataTablesParameters param)
        {
            try
            {

                // Sort Column Name  
                var sortColumn = param.SortOrder;  //["columns[" + param["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();

                // Sort Column Direction (asc, desc)  
                var sortColumnDirection = param.Order[0].Dir;// (param.SortOrder); //["order[0][dir]"].FirstOrDefault();

                // Search Value from (Search box)  
                var searchValue = param.Search.Value; //["search[value]"].FirstOrDefault();

               
                List<string> AdditionalValues = param.AdditionalValues.ToList();
                var StateFiler = AdditionalValues[0].ToString();
                
                // getting all Customer data  
                var appUsers = _userManager.Users;


                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    appUsers = appUsers.OrderBy(sortColumn );
                }

                //State  
                if (!string.IsNullOrEmpty(StateFiler))
                {
                    if (StateFiler == "true")
                        appUsers = appUsers.Where(m => m.IsActive == true);
                    
                }

                
                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    appUsers = appUsers.Where(m => m.PersonName.ToLower().Contains(searchValue.ToLower()) || m.Email.ToLower().Contains(searchValue.ToLower())
                    || m.UserName.ToLower().Contains(searchValue.ToLower()) || m.AccessFailedCount.ToString().ToLower().Contains(searchValue.ToLower())
                    
                    || m.EmailConfirmed.ToString().ToLower().Contains(searchValue.ToLower()) || m.LockoutEnabled.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.PhoneNumberConfirmed.ToString().ToLower().Contains(searchValue.ToLower()) || m.LockoutEnd.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.Remarks.ToLower().Contains(searchValue.ToLower()) 
                    
                    );
                }          

                return appUsers;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<UserViewModel> GetViewModel(IQueryable<ApplicationUser> applicationUsers)
        {
            List<UserViewModel> userViewModels = new List<UserViewModel>();

            foreach (ApplicationUser appUser in applicationUsers)
            {
                UserViewModel userViewModel = new UserViewModel()
                {
                    AccessFailedCount = appUser.AccessFailedCount,
                    Email = appUser.Email,
                    Id = appUser.Id,                    
                    IsActive = appUser.IsActive,
                    IsEmailConfirmed = appUser.EmailConfirmed,
                    IsLockOutEnabled = appUser.LockoutEnabled,
                    IsPhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
                    LockOutEndTime = appUser.LockoutEnd,
                    PersonName = appUser.PersonName,
                    Remarks = appUser.Remarks,  
                    Theme = appUser.Theme,
                    UserName = appUser.UserName,
                    PhoneNumber=appUser.PhoneNumber,
                    IsUserAdmin = _userManager.GetRolesAsync(appUser).Result.Contains("Admin")
                };

                userViewModels.Add(userViewModel);


            }

            return userViewModels;
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetExcel()
        {
            var param = HttpContext.Session.GetString(nameof(JqueryDataTablesParameters));

            //Returning Json Data  
            List<UserViewModel> userViewModels = GetViewModel(GetAppUsersData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));
            return Json(new { data = userViewModels });

        }

        [Authorize(Roles = "Admin")]
        public string ChangeUserState(string Id)
        {

            var appUser = _userManager.Users.Where(u => u.Id == Id).FirstOrDefault();

            appUser.IsActive = (!appUser.IsActive);

            var result = _userManager.UpdateAsync(appUser).Result.ToString();

            return result;
        }

        [Authorize(Roles = "Admin")]
        public async Task<string> ResetUserPassword(string Id)
        {
            var appUser = _userManager.Users.Where(u => u.Id == Id).FirstOrDefault();
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(appUser, resetToken, app.DefaultUserResetPassword);
            return passwordChangeResult.ToString();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateUser(string Id)
        {
            if (Id == null)
            {
                return NotFound(_localizationService.GetLocalized("Unable to load user."));
            }
            
            var appUser = _userManager.Users.Where(u => u.Id == Id).FirstOrDefault();

            if (appUser == null)
            {
                return NotFound(_localizationService.GetLocalized("Unable to load user."));
            }

            
            
                
            UserViewModel model = new UserViewModel()
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                IsEmailConfirmed = appUser.EmailConfirmed,
                PersonName = appUser.PersonName,
                PhoneNumber = appUser.PhoneNumber,
                IsPhoneNumberConfirmed = appUser.PhoneNumberConfirmed,               
                Remarks = appUser.Remarks,
                IsActive = appUser.IsActive,
                Theme = appUser.Theme,
                AccessFailedCount = appUser.AccessFailedCount, 
                //IsLockOutEnabled = appUser.LockoutEnabled,
                LockOutEndTime = appUser.LockoutEnd,              
                ExistingPhoto = appUser.UserPhoto,
                IsUserAdmin = _userManager.GetRolesAsync(appUser).Result.Contains("Admin")

        };
           
            return View(model);
        }

        // POST: Sites/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string Id, UserViewModel updatedUser)
        {
    
            if (Id == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Invalid call of the service."));
                return View(updatedUser);
               
            }

            var usr = _userManager.Users.Where(u => u.Id == Id).FirstOrDefault();
            if (usr == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Specified user is not found."));
                return View(updatedUser);               
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (updatedUser.RemoveImage)
                    {
                        usr.UserPhoto = null;
                    }

                    //Id = updatedUser.Id,
                    //NewUserPhoto
                    using (var memoryStream = new MemoryStream())
                    {
                        if (updatedUser.UserPhoto != null)
                        {
                            if (updatedUser.UserPhoto.ContentType != "image/jpeg")
                            {
                                ModelState.AddModelError("File", _localizationService.GetLocalized("The File Type is not permitted. Only Jpeg format is allowed."));
                                return View(updatedUser);
                            }
                            if (updatedUser.UserPhoto.Length > (100 * 1024)) // 5KB
                            {
                                ModelState.AddModelError("File", _localizationService.GetLocalized("The file is too large. Maximum allowed size is 100 KB."));
                                return View(updatedUser);
                            }

                            updatedUser.UserPhoto.CopyTo(memoryStream);
                            usr.UserPhoto = memoryStream.ToArray();
                        }
                    }

                    usr.UserName = updatedUser.UserName;
                    usr.Email = updatedUser.Email;
                    usr.IsActive = updatedUser.IsActive;
                    usr.PersonName = updatedUser.PersonName;
                    usr.PhoneNumber = updatedUser.PhoneNumber;
                    usr.Theme = updatedUser.Theme;
                    usr.Remarks = updatedUser.Remarks;

                    usr.EmailConfirmed = updatedUser.IsEmailConfirmed;
                    usr.PhoneNumberConfirmed = updatedUser.IsPhoneNumberConfirmed;
                    usr.AccessFailedCount = updatedUser.AccessFailedCount;
                    usr.LockoutEnabled = updatedUser.IsLockOutEnabled;
                    //usr.LockoutEnd = updatedUser.LockOutEndTime;                   
                    

                    var result =  _userManager.UpdateAsync(usr).Result;

                    if (result.Succeeded)
                    {
                        // remove roles and add new 
                        //var roles = _context.Roles.Where(u => !u.Name.Contains("Admin")).Select(u => u.Name).ToList();
                        //await _userManager.RemoveFromRolesAsync(usr, roles);

                        //// Add selected Role
                        //await _userManager.AddToRoleAsync(usr, updatedUser.UserRoles);
                        if (updatedUser.IsUserAdmin)
                        {
                            await _userManager.AddToRoleAsync(usr, "Admin");
                        }
                        else {
                            await _userManager.RemoveFromRoleAsync(usr, "Admin");
                        }

                        return RedirectToAction("AppUsers");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }


                     return View(updatedUser);
                }
                catch (DbUpdateConcurrencyException)
                {                    
                        throw;                    
                }
            }
            return View(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        public async Task<string> SendVerificationEmail(string Id)
        {
            
            if (app.IsSystemEmailEnabled && app.IsEmailConfirmationRequired)
            {
            
                var user = await _userManager.FindByIdAsync(Id);
                if (user == null)
                {
                    return _localizationService.GetLocalized("User Not Found");
                }

                var userId = Id;
                var email = await _userManager.GetEmailAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                var AppUrl = new Uri($"{Request.Scheme}://{Request.Host}").ToString();

                var msgbody = _emailSender.TemplateAccountActivation(user.UserName, user.PersonName, HtmlEncoder.Default.Encode(callbackUrl), AppUrl);

                await _emailSender.SendEmailAsync(
                    email,
                    _localizationService.GetLocalized("SIDCs - Confirm your email"),
                    msgbody);

                return "Succeeded";
                
            }
            else
            {
                return _localizationService.GetLocalized("Email sending system is disabled by admin. Please contact administrator.");
            }           

        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(string Id)
        {
            if (Id == null)
            {
                return NotFound(_localizationService.GetLocalized("Unable to load user."));
            }

            var appUser = _userManager.Users
                .Where(u => u.Id == Id)               
                .FirstOrDefault();

            if (appUser == null)
            {
                return NotFound(_localizationService.GetLocalized("Unable to load user."));
            }

            
           
            UserViewModel model = new UserViewModel()
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                IsEmailConfirmed = appUser.EmailConfirmed,
                PersonName = appUser.PersonName,
                PhoneNumber = appUser.PhoneNumber,
                IsPhoneNumberConfirmed = appUser.PhoneNumberConfirmed,                
                Remarks = appUser.Remarks,
                IsActive = appUser.IsActive,
                Theme = appUser.Theme,
                AccessFailedCount = appUser.AccessFailedCount,
                IsLockOutEnabled = appUser.LockoutEnabled,
                LockOutEndTime = appUser.LockoutEnd
               

            };

            return View(model);
        }

        
        public IActionResult Error(ErrorViewModel model)
        {
            return View(model);
        }

    }

}
