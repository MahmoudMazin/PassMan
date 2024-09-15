using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PassManNew.Data;
using PassManNew.Models;
using Microsoft.AspNetCore.Authorization;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Identity;
using PassManNew.Resources;


namespace PassManNew.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAppSettings app;
        private readonly LocalizationService _localizationService;
        public DocumentsController(LocalizationService localizationService, IAppSettings _app, ApplicationDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            app = _app;
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _localizationService = localizationService;
        }

        // GET: Locations
        public IActionResult Index()
        {
            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName");
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
                IQueryable<Doc> data = GetData(param);

                //total number of rows counts   
                recordsTotal = data.Count();

                pageSize = (pageSize == -1) ? recordsTotal : pageSize;

                //Paging   
                data = data.Skip(skip).Take(pageSize);

                DocSharingViewModel ViewModels = GetViewModel(data);



                //Returning Json Data  
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = ViewModels });

            }
            catch (Exception)
            {
                throw;
            }

            // return View();
        }

        private DocSharingViewModel GetViewModel(IQueryable<Doc> Docs)
        {
            DocSharingViewModel ViewModels = new DocSharingViewModel();

            foreach (Doc item in Docs)
            {   
                ViewModels.Docs.Add(item);
            }

            return ViewModels;
        }


        private IQueryable<Doc> GetData(JqueryDataTablesParameters param)
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

                // getting all User Docs
                IQueryable<Doc> data = _context.Docs.Include(r => r.Owner)
                    .Include(r => r.DocPermissions)
                    .Select(x => new Doc {
                        ContentType = x.ContentType,
                        DocPermissions = x.DocPermissions,
                        FileName = x.FileName,
                        FileTitle = x.FileTitle,
                        FileType = x.FileType,
                        Id = x.Id,
                        IsPublic = x.IsPublic,
                        Owner = x.Owner,
                        OwnerId = x.OwnerId,
                        UploadDateTime = x.UploadDateTime,
                        Size = string.Format("{0} MB", (Convert.ToDecimal(x.Content.Length) / Convert.ToDecimal(1048576.00)).ToString("F2")),
                        Content = null,
                        UserCanModify = (x.OwnerId == _userManager.GetUserId(User) || User.IsInRole("Admin"))?true: false
                    })
                    .Where(u => u.IsPublic ||
                    u.DocPermissions.Where(dp => dp.DocId == u.Id && dp.PersonId == _userManager.GetUserId(User)).Any());


                //Sorting  
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection.ToString())))
                {
                    data = data.OrderBy(sortColumn);
                }

                //State  
                if (!string.IsNullOrEmpty(StateFiler))
                {
                    if (StateFiler == "false")
                        data = data.Where(m => m.OwnerId == _userManager.GetUserId(User));

                }

                //Search  
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(m => m.FileName.ToLower().Contains(searchValue.ToLower())
                    || m.FileTitle.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.FileType.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.UploadDateTime.ToString("dd-MMM-yy hh:mm").ToLower().Contains(searchValue.ToLower())
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

            //Returning Json Data  
            DocSharingViewModel ViewModel = GetViewModel(GetData(JsonConvert.DeserializeObject<JqueryDataTablesParameters>(param)));

            return Json(new { data = ViewModel.Docs });

        }

       
        public async Task<string> Delete(Guid Id)
        {
            try
            {
                Doc doc = _context.Docs.Where(u => u.Id == Id).FirstOrDefault();
                if (doc != null && (User.IsInRole("Admin") || doc.OwnerId == _userManager.GetUserId(User) ))
                {
                    var docpermissions = _context.DocPermissions.Where(u => u.DocId == Id);
                    if (docpermissions.Any())
                    {
                        _context.DocPermissions.RemoveRange(docpermissions);
                    }

                    if (doc != null)
                    {
                        _context.Docs.Remove(doc);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return _localizationService.GetLocalized("Either Document Not Found or You dont have Ownership.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return _localizationService.GetLocalized("Error in deleting document. Please contact Administrator");
            }

            return "Succeeded";
        }


        // POST: Shifts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(DocSharingViewModel model) //[Bind("Id,Name,StartTime,EndTime,Description,State")]
        {
            List<string> permittedExtensions = _configuration.GetSection("PermittedUploadFileTypes:FileTypes").Get<List<string>>();
            using (var memoryStream = new MemoryStream())
            {
              
                if (model.FormFile != null)
                {
                    if (!permittedExtensions.Any(e => model.FormFile.FileName.EndsWith(e)))
                    {
                        ModelState.AddModelError("File", _localizationService.GetLocalized("The File Type is not permitted."));
                        return View("Index", model);
                    }
                    if (model.FormFile.Length > (app.UploadFileSize * 1024 * 1024))
                    {
                        ModelState.AddModelError("File", string.Format(_localizationService.GetLocalized("The file is too large. Maximum allowed size is {0} MB."), app.UploadFileSize));
                        return View("Index", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("File", _localizationService.GetLocalized("Please attach the document."));
                    return View("Index", model);
                }

                await model.FormFile.CopyToAsync(memoryStream);
                var file = new Doc()
                {
                    Content = memoryStream.ToArray(),
                    FileTitle = model.Doc.FileTitle,
                    FileName = model.FormFile.FileName,
                    IsPublic = model.Doc.IsPublic,
                    ContentType = model.FormFile.ContentType,
                    FileType = model.Doc.FileType,
                    UploadDateTime = System.DateTime.Now,
                    OwnerId = _userManager.GetUserId(User)
                };

                _context.Docs.Add(file);
                await _context.SaveChangesAsync();


                if (!model.Doc.IsPublic && file.Id != null)
                {
                    bool result = SetPermissions(file.Id, model.AuthorizeUsers);
                    if (result)
                    {
                        ModelState.AddModelError("File", _localizationService.GetLocalized("Permission has been set successfully."));
                    }                   
                }
                else
                {
                    ModelState.AddModelError("File", _localizationService.GetLocalized("File has been successfully uploaded."));
                }


            }

            ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName");
            return View("Index");
        }

        private bool ValidateFile()
        {

            return true;
        }
        private bool SetPermissions(Guid id, IEnumerable<string> AuthorizeUsers)
        {
            try
            {
                List<DocPermission> OldPermissions = _context.DocPermissions.Where(u => u.DocId == id).ToList();
                if (OldPermissions.Any())
                {
                    _context.DocPermissions.RemoveRange(OldPermissions);
                    _context.SaveChanges();
                }

                List<DocPermission> NewDocPermistion = new List<DocPermission>();

                // Add Owner in the list
                NewDocPermistion.Add(new DocPermission { DocId = id, PersonId = _userManager.GetUserId(User) });

                foreach (string user in AuthorizeUsers)
                {
                    if (user != _userManager.GetUserId(User))
                        NewDocPermistion.Add(new DocPermission { DocId = id, PersonId = user });
                }

                _context.DocPermissions.AddRange(NewDocPermistion);
                _context.SaveChanges();               
                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", _localizationService.GetLocalized("There is an error in setting Permissions.") + ex.Message);
                return false;
            }


        }

        public ActionResult DownloadDocument(Guid id)

        {
            if (id == null)
                return NotFound();

            var fileToRetrieve = _context.Docs.Find(id);
            if (fileToRetrieve == null)
                return NotFound();

            if (fileToRetrieve.IsPublic || UserHavePermission(_userManager.GetUserId(User), id))
            {
                return File (fileToRetrieve.Content, fileToRetrieve.ContentType, fileToRetrieve.FileName);
            }
            else
                return Unauthorized();
        }

        private bool UserHavePermission(string userid, Guid DocId)
        {
            if (_context.DocPermissions.Where(f => f.DocId == DocId && f.PersonId == userid).Any())
                return true;
            else
                return false;
        }


        // GET: Shifts/Edit/5
        public ActionResult Edit(Guid? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            try
            {
                Doc doc = _context.Docs.Where(u => u.Id == Id)
                    .Include(u=>u.DocPermissions)
                    .FirstOrDefault();

                if (doc != null && (User.IsInRole("Admin") || doc.OwnerId == _userManager.GetUserId(User)))
                {
                    ICollection<DocPermission> docpermissions = _context.DocPermissions.Where(u => u.DocId == Id).ToList();
                    if (docpermissions.Any())
                    {
                        doc.DocPermissions = docpermissions;   
                        
                    }
                    DocSharingViewModel model = new DocSharingViewModel();
                    doc.Size = string.Format("{0} MB", (Convert.ToDecimal(doc.Content.Length) / Convert.ToDecimal(1048576.00)).ToString("F2"));
                    doc.Content = null;
                    model.Doc = doc;
                    model.AuthorizeUsers = doc.DocPermissions.Select(r => r.PersonId).ToList();
                    ViewData["UsersList"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.IsActive == true), "Id", "PersonName",model.AuthorizeUsers);
                    return View(model);
                }
                else
                {
                    return Unauthorized();                                     
                }
            }
            catch (Exception ex)
            {                
                return BadRequest(ex.Message);
            }

            
        }

        // POST: Shifts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DocSharingViewModel model) //[Bind("Id,Name,StartTime,EndTime,Description,State")]
        {

            if (id != model.Doc.Id)
            {
                return NotFound();
            }

            List<string> permittedExtensions = _configuration.GetSection("PermittedUploadFileTypes:FileTypes").Get<List<string>>();
            using (var memoryStream = new MemoryStream())
            {
                
                if (model.FormFile != null)
                {
                    if (!permittedExtensions.Any(e => model.FormFile.FileName.EndsWith(e)))
                    {
                        ModelState.AddModelError("File",_localizationService.GetLocalized("The File Type is not permitted."));
                        return View("Index", model);
                    }
                    if (model.FormFile.Length > (app.UploadFileSize * 1024 * 1024))                    {
                       
                        ModelState.AddModelError("File", string.Format(_localizationService.GetLocalized("The file is too large. Maximum allowed size is {0} MB."), app.UploadFileSize));
                        return View("Index", model);
                    }

                    await model.FormFile.CopyToAsync(memoryStream);
                }


                var file = _context.Docs.Where(u => u.Id == id).Single();
                if (model.FormFile != null)
                {
                    file.Content = memoryStream.ToArray();
                    file.ContentType = model.FormFile.ContentType;
                    file.FileName = model.FormFile.FileName;
                }
                

                file.FileTitle = model.Doc.FileTitle;               
                file.IsPublic = model.Doc.IsPublic;
                file.FileType = model.Doc.FileType;
                file.UploadDateTime = System.DateTime.Now;
                file.OwnerId = _userManager.GetUserId(User);
                

                _context.Docs.Update(file);
                await _context.SaveChangesAsync();


                if (!model.Doc.IsPublic && file.Id != null)
                {
                    bool result = SetPermissions(file.Id, model.AuthorizeUsers);
                    if (result)
                    {
                        ModelState.AddModelError("File", _localizationService.GetLocalized("File has been uploaded and permission has been set successfully."));
                    }
                }
                else
                {
                    ModelState.AddModelError("File", _localizationService.GetLocalized("File has been successfully uploaded."));
                }


            }


            return RedirectToAction("Edit", routeValues: new { id, model });
        }

       
    }
}

