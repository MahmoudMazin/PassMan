
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PassManNew.Data;
using PassManNew.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;
using PassManNew.Resources;
using Microsoft.AspNetCore.Authorization;

namespace PassManNew.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WebController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LocalizationService _localizationService;

        public WebController(LocalizationService localizationService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _localizationService = localizationService;
        }

        // GET: Web
        public IActionResult Index()
        {

            return View();
        }


        [HttpPost]
        public IActionResult Index([FromBody] JqueryDataTablesParameters param)
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
                IQueryable<Web> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);


                //Returning Json Data  
                List<WebViewModel> WebViewModels = GetViewModel(data);


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = WebViewModels });

            }
            catch (Exception)
            {
                throw;
            }


        }

        private List<WebViewModel> GetViewModel(IQueryable<Web> Webs)
        {
            List<WebViewModel> WebViewModels = new List<WebViewModel>();

            foreach (Web item in Webs)
            {
                WebViewModel curitem = new WebViewModel();
              //  curitem.Id = item.Id;
                curitem.WebName = item.WebName;
                curitem.JsonRPA = item.JsonRPA;
                curitem.UserNameField = item.UserNameField;
                curitem.PwdNameField = item.PwdNameField;

                WebViewModels.Add(curitem);
            }

            return WebViewModels;
        }

        private IQueryable<Web> GetData(JqueryDataTablesParameters param)
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
                IQueryable<Web> data = Enumerable.Empty<Web>().AsQueryable();

                data = _context.Webs;


                //if (CompanyFilter != "All")
                //{
                //    data = data.Where(u => u.CompanyId.ToString() == CompanyFilter); ;
                //}


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

                    data = data.OrderBy(sortColumn);
                }


                //State  


                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.WebName.ToLower().Contains(searchValue.ToLower())
                    || (m.WebName != null && m.WebName.ToString().ToLower().Contains(searchValue.ToLower()))
                    || m.UserNameField.ToLower().Contains(searchValue.ToLower())
                    || m.PwdNameField.ToString().ToLower().Contains(searchValue.ToLower())
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

            List<WebViewModel> WebViewModels = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            //Returning Json Data  
            return Json(new { data = WebViewModels });

        }





        public async Task<string> Delete(string Id)
        {

            var data = _context.Webs.Include(u => u.Links).Where(u => u.WebName == Id).FirstOrDefault();
            try
            {
                _context.Remove(data);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return _localizationService.GetLocalized("Error: ") + ex.Message;
            }

            return "Succeeded";
        }

        // GET: Webs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Webs
                .Include(r => r.Links)
                .FirstOrDefaultAsync(m => m.WebName == id);

            if (item == null)
            {
                return NotFound();
            }

            WebViewModel curitem = new WebViewModel();

            curitem.JsonRPA = item.JsonRPA;
            curitem.WebName = item.WebName;
            curitem.UserNameField = item.UserNameField;
            curitem.PwdNameField = item.PwdNameField;
         
            return View(curitem);
        }

        // GET: Webs/Create       
        public IActionResult Create(string id)
        {
            
            return View();
        }

        // POST: Webs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebViewModel item)
        {
            if (ModelState.IsValid)
            {
                Web curitem = new Web();
             
                curitem.WebName = item.WebName;
                curitem.JsonRPA = item.JsonRPA;
                curitem.UserNameField = item.UserNameField;
                curitem.PwdNameField = item.PwdNameField;

            
                _context.Add(curitem);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

       
            return View(item);
        }

        // GET: Webs/Edit/5       
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = _context.Webs
                .Include(r => r.Links)
                .Where(u => u.WebName == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            WebViewModel curitem = new WebViewModel();
  
            curitem.WebName = item.WebName;
            curitem.JsonRPA = item.JsonRPA;
            curitem.UserNameField = item.UserNameField;
            curitem.PwdNameField = item.PwdNameField;
            return View(curitem);
        }


        // POST: Webs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WebViewModel item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    // To remove Old SalesMan from Saleman role
                    Web curitem = _context.Webs.Where(u => u.WebName == item.WebName).FirstOrDefault();

                    
                    curitem.WebName = item.WebName;
                    curitem.JsonRPA = item.JsonRPA;
                    curitem.UserNameField = item.UserNameField;
                    curitem.PwdNameField = item.PwdNameField;


                    _context.Update(curitem);


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }



        private bool WebExists(string id)
        {
            return _context.Webs.Any(e => e.WebName == id);
        }
    }
}
