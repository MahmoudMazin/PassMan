using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PassManNew.Data;
using Microsoft.AspNetCore.Identity;


namespace PassManNew.Models
{
    //Entity Class
    [Table("Setting")]
    public partial class Setting
    {      
        public Guid Id { get; set; }   
        

        [Display(Name = "Upload File Size")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [DefaultValue(5)]
        public int UploadFileSize { get; set; }

        [Display(Name = "Is Email Enabled?")]
        [DefaultValue(true)]
        public bool IsSystemEmailEnabled { get; set; }

        [Display(Name = "Is Email Confirmation Required?")]
        [DefaultValue(true)]
        public bool IsEmailConfirmationRequired { get; set; }

        [Display(Name = "Is Reset Password Email Required?")]
        [DefaultValue(true)]
        public bool IsResetPasswordEmailRequired { get; set; }

        [Display(Name = "Is Permit Email Required?")]
        [DefaultValue(true)]
        public bool IsPermitEmailRequired { get; set; }

        

        //Email Configuration Parameters     


        [Display(Name = "Smtp Server")]
        public string SmtpServer { get; set; }

        [Display(Name = "Smtp Port")]
        public int SmtpPort { get; set; }

        [Display(Name = "Smtp User Name")]
        public string SmtpUserName { get; set; }

        [Display(Name = "Sender Name")]
        public string SenderName { get; set; }

        [Display(Name = "Smtp Password")]
        public string SmtpPassword { get; set; }


        [Display(Name = "Pop Server")]
        public string PopServer { get; set; }

        [Display(Name = "Pop Port")]
        public int PopPort { get; set; }

        [Display(Name = "Pop User Name")]
        public string PopUserName { get; set; }

        [Display(Name = "Pop Password")]
        public string PopPassword { get; set; }



        //Google API parameters

        [Display(Name = "Is GoogleMap Enabled?")]
        public bool GoogleApiState { get; set; }

        [Display(Name = "Google API Key")]
        public string GoogleApi { get; set; }

     

        //Default User Settings

        [Display(Name = "Default Admin User Name")]
        public string DefaultAdminUserName { get; set; }

        [Display(Name = "Default Admin User Email")]
        public string DefaultAdminUserEmail { get; set; }

        [Display(Name = "Default Admin Password")]
        public string DefaultAdminUserPassword { get; set; }

        [Display(Name = "Default User Reset Password")]
        public string DefaultUserResetPassword { get; set; }



    }

    // ViewModel for Setting UI 
    public class SettingViewModel
    {
        public Guid Id { get; set; }
        

        [Display(Name = "Upload File Size")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [DefaultValue(5)]
        public int UploadFileSize { get; set; }

        [Display(Name = "Is Email Enabled?")]
        [DefaultValue(true)]
        public bool IsSystemEmailEnabled { get; set; }

        [Display(Name = "Is Email Confirmation Required?")]
        [DefaultValue(true)]
        public bool IsEmailConfirmationRequired { get; set; }

        [Display(Name = "Is Reset Password Email Required?")]
        [DefaultValue(true)]
        public bool IsResetPasswordEmailRequired { get; set; }

        [Display(Name = "Is Permit Email Required?")]

        [DefaultValue(true)]
        public bool IsPermitEmailRequired { get; set; }


        //Email Configuration Parameters     


        [Display(Name = "Smtp Server")]
        public string SmtpServer { get; set; }

        [Display(Name = "Smtp Port")]
        public int SmtpPort { get; set; }

        [Display(Name = "Smtp User Name")]
        public string SmtpUserName { get; set; }

        [Display(Name = "Sender Name")]
        public string SenderName { get; set; }

        [Display(Name = "Smtp Password")]
        public string SmtpPassword { get; set; }


        [Display(Name = "Pop Server")]
        public string PopServer { get; set; }

        [Display(Name = "Pop Port")]
        public int PopPort { get; set; }

        [Display(Name = "Pop User Name")]
        public string PopUserName { get; set; }

        [Display(Name = "Pop Password")]
        public string PopPassword { get; set; }








        //Google API parameters

        [Display(Name = "Is GoogleMap Enabled?")]
        public bool GoogleApiState { get; set; }

        [Display(Name = "Google API Key")]
        public string GoogleApi { get; set; }





        //Default User Settings

        [Display(Name = "Default Admin User Name")]
        public string DefaultAdminUserName { get; set; }

        [Display(Name = "Default Admin User Email")]
        public string DefaultAdminUserEmail { get; set; }

        [Display(Name = "Default Admin Password")]
        public string DefaultAdminUserPassword { get; set; }

        [Display(Name = "Default User Reset Password")]
        public string DefaultUserResetPassword { get; set; }




    }

    // Appsetting as Static Object.

    public interface IAppSettings {
        Guid Id { get; }
       

        int UploadFileSize { get;  set;}

        bool IsSystemEmailEnabled { get;  set;}

        bool IsEmailConfirmationRequired { get;  set;}

        bool IsResetPasswordEmailRequired { get;  set;}

        bool IsPermitEmailRequired { get;  set;}

           

        string SmtpServer { get;  set; }

        int SmtpPort { get;  set;}

        string SmtpUserName { get;  set;  }

        string SenderName { get;  set;  }

        string SmtpPassword { get;  set;  }


        string PopServer { get;  set;  }

        int PopPort { get;  set;  }

        string PopUserName { get;  set;  }

        string PopPassword { get;  set;  }


        bool GoogleApiState { get;  set;  }

       string GoogleApi { get;  set;  }



        string DefaultAdminUserName { get;  set;  }

        string DefaultAdminUserEmail { get;  set;  }

        string DefaultAdminUserPassword { get;  set;  }

        string DefaultUserResetPassword { get;  set;  }

    }

    public class AppSettings : IAppSettings    {
       
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IConfiguration _config;


        public AppSettings(IServiceScopeFactory scopeFactory, IConfiguration config)
        {           
            
            this.scopeFactory = scopeFactory;
            _config = config;

            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                 
                
                if (!_context.Settings.Any())
                {
                    Setting defsetting = new Setting();

                    defsetting.UploadFileSize = _config.GetValue<short>("DefaultAppSettings:UploadFileSize");
                    defsetting.IsSystemEmailEnabled = _config.GetValue<bool>("DefaultAppSettings:IsSystemEmailEnabled");
                    defsetting.IsEmailConfirmationRequired = _config.GetValue<bool>("DefaultAppSettings:IsEmailConfirmationRequired");
                    defsetting.IsResetPasswordEmailRequired = _config.GetValue<bool>("DefaultAppSettings:IsResetPasswordEmailRequired");
                    defsetting.IsPermitEmailRequired = _config.GetValue<bool>("DefaultAppSettings:IsPermitEmailRequired");

                    defsetting.SmtpServer = _config.GetValue<string>("EmailConfiguration:SmtpServer");
                    defsetting.SmtpPort = _config.GetValue<short>("EmailConfiguration:SmtpPort");
                    defsetting.SmtpUserName = _config.GetValue<string>("EmailConfiguration:SmtpUsername");
                    defsetting.SenderName = _config.GetValue<string>("EmailConfiguration:SenderName").ToString();
                    defsetting.SmtpPassword = _config.GetValue<string>("EmailConfiguration:SmtpPassword").ToString();
                    defsetting.PopServer = _config.GetValue<string>("EmailConfiguration:PopServer").ToString();
                    defsetting.PopPort = _config.GetValue<short>("EmailConfiguration:PopPort");
                    defsetting.PopUserName = _config.GetValue<string>("EmailConfiguration:PopUsername").ToString();
                    defsetting.PopPassword = _config.GetValue<string>("EmailConfiguration:PopPassword").ToString();

                    defsetting.GoogleApiState = _config.GetValue<bool>("GoogleApiSettings:IsActive");
                    defsetting.GoogleApi = _config.GetValue<string>("GoogleApiSettings:API").ToString();

                    defsetting.DefaultAdminUserName = _config.GetValue<string>("UserSettings:AdminUserName").ToString();
                    defsetting.DefaultAdminUserEmail = _config.GetValue<string>("UserSettings:AdminEmail").ToString();
                    defsetting.DefaultAdminUserPassword = _config.GetValue<string>("UserSettings:AdminPassword").ToString();
                    defsetting.DefaultUserResetPassword = _config.GetValue<string>("UserSettings:DefaultUserResetPwd").ToString();

                    _context.Add<Setting>(defsetting);
                    _context.SaveChanges();
                }

                Setting setting = _context.Settings.FirstOrDefault();

                Id = setting.Id;
                UploadFileSize = setting.UploadFileSize;
                IsSystemEmailEnabled = setting.IsSystemEmailEnabled;
                IsEmailConfirmationRequired = setting.IsEmailConfirmationRequired;
                IsResetPasswordEmailRequired = setting.IsResetPasswordEmailRequired;
                IsPermitEmailRequired = setting.IsPermitEmailRequired;
                SmtpServer = setting.SmtpServer;
                SmtpPort = setting.SmtpPort;
                SmtpUserName = setting.SmtpUserName;
                SenderName = setting.SenderName;
                SmtpPassword = setting.SmtpPassword;
                PopServer = setting.PopServer;
                PopPort = setting.PopPort;
                PopUserName = setting.PopUserName;
                PopPassword = setting.PopPassword;
                GoogleApiState = setting.GoogleApiState;
                GoogleApi = setting.GoogleApi;
                DefaultAdminUserName = setting.DefaultAdminUserName;
                DefaultAdminUserEmail = setting.DefaultAdminUserEmail;
                DefaultAdminUserPassword = setting.DefaultAdminUserPassword;
                DefaultUserResetPassword = setting.DefaultUserResetPassword;
            }
        }

        public Guid Id { get; }
        public int UploadFileSize { get;  set;}
        public bool IsSystemEmailEnabled { get;  set;}
        public bool IsEmailConfirmationRequired { get;  set;}
        public bool IsResetPasswordEmailRequired { get;  set;}
        public bool IsPermitEmailRequired { get;  set;}

        [Display(Name = "Smtp Server")]
        public string SmtpServer { get;  set; }

        [Display(Name = "Smtp Port")]
        public int SmtpPort { get;  set;}

        [Display(Name = "Smtp User Name")]
        public string SmtpUserName { get;  set;  }

        [Display(Name = "Sender Name")]
        public string SenderName { get;  set;  }

        [Display(Name = "Smtp Password")]
        public string SmtpPassword { get;  set;  }


        [Display(Name = "Pop Server")]
        public string PopServer { get;  set;  }

        [Display(Name = "Pop Port")]
        public int PopPort { get;  set;  }

        [Display(Name = "Pop User Name")]
        public string PopUserName { get;  set;  }

        [Display(Name = "Pop Password")]
        public string PopPassword { get;  set;  }


        //Google API parameters

        [Display(Name = "Is GoogleMap Enabled?")]
        public bool GoogleApiState { get;  set;  }

        [Display(Name = "Google API Key")]
        public string GoogleApi { get;  set;  }



        //Default User Settings

        [Display(Name = "Default Admin User Name")]
        public string DefaultAdminUserName { get;  set;  }

        [Display(Name = "Default Admin User Email")]
        public string DefaultAdminUserEmail { get;  set;  }

        [Display(Name = "Default Admin Password")]
        public string DefaultAdminUserPassword { get;  set;  }

        [Display(Name = "Default User Reset Password")]
        public string DefaultUserResetPassword { get;  set;  }
    }


}
