using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//using PassManNew.Areas.Identity.Pages.Account;
using PassManNew.Models;
using PassManNew.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PassManNew.Controllers
{
    public class ACountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginViewModel> _logger;
        private readonly LocalizationService _localizationService;
        private readonly IMyLog _MyLog;
       
        public ACountController(IMyLog MyLog, LocalizationService localizationService, SignInManager<ApplicationUser> signInManager, ILogger<LoginViewModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _MyLog = MyLog;

            _userManager = userManager;
            _localizationService = localizationService;
        }
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> SignIn()
        //{
        //    SignInViewModel model = new SignInViewModel();
        //    //var user =await  _userManager.FindByEmailAsync("aali@azards.com");
        //   // var us = await _userManager.FindByNameAsync(user.UserName);
        //    var result = await _signInManager.PasswordSignInAsync("admin", "123$Qwe", true, lockoutOnFailure: false);
        //    return RedirectToAction("Index", "Home");
        //    //return View(model);
        //}
        //[HttpPost]
        public async Task<IActionResult> SignIn()
        {
            SignInViewModel model = new SignInViewModel();
            model.Email = "aali@azards.com"; model.Password = "123$Qwe"; model.RememberMe = true;
            string returnUrl = Url.Action("Index", "Home");
            if (ModelState.IsValid)
            {

                if (model.Email.IndexOf('@') > -1)
                {
                    //Validate email format
                    string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                              @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                    Regex re = new Regex(emailRegex);
                    if (!re.IsMatch(model.Email))
                    {
                        ModelState.AddModelError("Email", _localizationService.GetLocalized("Email is not valid"));
                    }
                }
                else
                {
                    //validate Username format
                    string emailRegex = @"^[a-zA-Z0-9]*$";
                    Regex re = new Regex(emailRegex);
                    if (!re.IsMatch(model.Email))
                    {
                        ModelState.AddModelError("Email", _localizationService.GetLocalized("Username is not valid"));
                    }
                }

                if (ModelState.IsValid)
                {
                    var userName = model.Email;

                    if (userName.IndexOf('@') > -1)
                    {
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user == null)
                        {
                            ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Invalid login attempt."));
                            return View();
                        }
                        else
                        {
                            userName = user.UserName;
                        }

                    }

                    //To Check if User is Active
                    var usr = await _userManager.FindByNameAsync(userName);

                    if (usr == null)
                    {
                        ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("User Not Found."));
                        return View();
                    }
                    if (!usr.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Your Account is Disabled."));
                        return View();
                    }

                    var result = await _signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: true);

                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        _MyLog.CreateLog(Models.Shared.LogType.Login, "Logged In", userName);

                        _logger.LogInformation(_localizationService.GetLocalized("User logged in."));


                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(_localizationService.GetLocalized("User account locked out."));
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localizationService.GetLocalized("Invalid login attempt."));
                        return View();
                    }
                }
            }
            return View();
        }
    }
}
