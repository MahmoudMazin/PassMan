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
//using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
//using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;
using PassManNew.Services.EmailService;
using PassManNew.Resources;

namespace PassManNew.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppSettings _app;
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly LocalizationService _localizationService;

        public AppSettingsController(LocalizationService localizationService,IEmailConfiguration emailConfiguration, IAppSettings app, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _emailConfiguration = emailConfiguration;
            _app = app;
            _context = context;
            _userManager = userManager;
            _localizationService = localizationService;
        }

        // GET: Shifts
        public IActionResult Index()
        {
            Setting setting = _context.Settings.FirstOrDefault();

            SettingViewModel settingVm = new SettingViewModel();
            settingVm.Id = setting.Id;                
            settingVm.UploadFileSize = setting.UploadFileSize;
            settingVm.IsSystemEmailEnabled = setting.IsSystemEmailEnabled;
            settingVm.IsEmailConfirmationRequired = setting.IsEmailConfirmationRequired;
            settingVm.IsResetPasswordEmailRequired = setting.IsResetPasswordEmailRequired;
            settingVm.IsPermitEmailRequired = setting.IsPermitEmailRequired;
            settingVm.GoogleApiState = setting.GoogleApiState;
            settingVm.GoogleApi = setting.GoogleApi;

            settingVm.DefaultAdminUserEmail= setting.DefaultAdminUserEmail;
            settingVm.DefaultAdminUserName = setting.DefaultAdminUserName;
            settingVm.DefaultAdminUserPassword= setting.DefaultAdminUserPassword;
            settingVm.DefaultUserResetPassword= setting.DefaultUserResetPassword;

            settingVm.SmtpServer= setting.SmtpServer;
            settingVm.SmtpUserName= setting.SmtpUserName;
            settingVm.SmtpPort= setting.SmtpPort;
            settingVm.SmtpPassword = setting.SmtpPassword;
            settingVm.SenderName = setting.SenderName;
            settingVm.PopServer= setting.PopServer;
            settingVm.PopPort= setting.PopPort;
            settingVm.PopUserName= setting.PopUserName;
            settingVm.PopPassword= setting.PopPassword;           

        
            return View(settingVm);
        }


        [HttpPost]       
        public async Task<IActionResult> Index(SettingViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = _context.Settings.Find(settings.Id);
                    
                
                    _app.UploadFileSize = data.UploadFileSize = settings.UploadFileSize;
                    _app.IsSystemEmailEnabled =  data.IsSystemEmailEnabled = settings.IsSystemEmailEnabled;
                   _app.IsEmailConfirmationRequired = data.IsEmailConfirmationRequired = settings.IsEmailConfirmationRequired;
                   _app.IsResetPasswordEmailRequired= data.IsResetPasswordEmailRequired = settings.IsResetPasswordEmailRequired;
                   _app.IsPermitEmailRequired = data.IsPermitEmailRequired = settings.IsPermitEmailRequired;


                    _app.GoogleApiState =  data.GoogleApiState = settings.GoogleApiState;
                    _app.GoogleApi = data.GoogleApi = settings.GoogleApi;

                    _app.DefaultAdminUserEmail = data.DefaultAdminUserEmail = settings.DefaultAdminUserEmail;
                    _app.DefaultAdminUserName =  data.DefaultAdminUserName = settings.DefaultAdminUserName;
                    _app.DefaultAdminUserPassword = data.DefaultAdminUserPassword = settings.DefaultAdminUserPassword;
                    _app.DefaultUserResetPassword = data.DefaultUserResetPassword = settings.DefaultUserResetPassword;

                    _emailConfiguration.SmtpServer = _app.SmtpServer =  data.SmtpServer = settings.SmtpServer;
                    _emailConfiguration.SmtpUsername= _app.SmtpUserName =  data.SmtpUserName = settings.SmtpUserName;
                    _emailConfiguration.SmtpPort= _app.SmtpPort = data.SmtpPort = settings.SmtpPort;
                    _emailConfiguration.SmtpPassword= _app.SmtpPassword = data.SmtpPassword = settings.SmtpPassword;
                    _emailConfiguration.SenderName= _app.SenderName = data.SenderName = settings.SenderName;
                    _emailConfiguration.PopServer= _app.PopServer = data.PopServer = settings.PopServer;
                    _emailConfiguration.PopPort= _app.PopPort = data.PopPort = settings.PopPort;
                    _emailConfiguration.PopUsername= _app.PopUserName = data.PopUserName = settings.PopUserName;
                    _emailConfiguration.PopPassword= _app.PopPassword = data.PopPassword = settings.PopPassword;

                    _context.Update(data);
                    await _context.SaveChangesAsync();

                    
                    ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Settings has been saved."));
                    
                }
                catch(Exception e) {

                    ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("There is an error. ") + e.Message );

                }

            }
                

        
            return View(settings);
        }

        private async Task RemoveRole(string oldId, string newId, string role)
        {
            if (oldId != null && oldId != "" && newId != oldId)
            {
                ApplicationUser old = await _userManager.FindByIdAsync(oldId);
                if (await _userManager.IsInRoleAsync(old, role))
                    await _userManager.RemoveFromRoleAsync(old, role);                
            }
        }

        private async Task AddRole(string oldId, string newId, string role)
        {
            if (newId != null && newId != "" && newId != oldId)
            {
                ApplicationUser newUser = await _userManager.FindByIdAsync(newId);
                if (await _userManager.IsInRoleAsync(newUser, role) == false)
                    await _userManager.AddToRoleAsync(newUser, role);                
            }
        }



    }
}
