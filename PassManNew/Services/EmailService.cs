using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PassManNew.Models;
using MailKit.Security;
using PassManNew.Resources;

namespace PassManNew.Services.EmailService
{
    public class EmailTemplates
    {            
    }    
    public class EmailAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
            CCAddresses = new List<EmailAddress>();
            BCCAddresses = new List<EmailAddress>();
        }

        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public List<EmailAddress> CCAddresses { get; set; }
        public List<EmailAddress> BCCAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }

    public interface IEmailConfiguration
    {
        string SmtpServer { get; set; }
        int SmtpPort { get; set; }
        string SmtpUsername { get; set; }
        string SenderName { get; set; }
        string SmtpPassword { get; set; }

        string PopServer { get; set; }
        int PopPort { get; set; }
        string PopUsername { get; set; }
        string PopPassword { get; set; }
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        private readonly IAppSettings app;

        public EmailConfiguration(IAppSettings _app) {
            app = _app;
            SmtpServer = app.SmtpServer;
            SmtpPort = app.SmtpPort;
            SmtpPassword = app.SmtpPassword;
            SmtpUsername = app.SmtpUserName;
            SenderName = app.SenderName;
            PopServer = app.PopServer;
            PopPort = app.PopPort;
            PopUsername = app.PopUserName;
            PopPassword = app.PopPassword;
        }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SenderName { get; set; }
        public string SmtpPassword { get; set; }

        public string PopServer { get; set; }
        public int PopPort { get; set; }
        public string PopUsername { get; set; }
        public string PopPassword { get; set; }
    }
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string htmlMessage);
        Task<bool> SendAsync(EmailMessage emailMessage);
        List<EmailMessage> ReceiveEmail(int maxCount = 10);
        string TemplateForgetPassword(string UserName, string ResetUrl, string ImagePath,string AppUrl);
        string TemplateAccountActivation(string UserName, string Name, string ActivateUrl, string AppUrl);

    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly LocalizationService _localizationService;

        public EmailService(IEmailConfiguration emailConfiguration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, LocalizationService localizationService)
        {
            _emailConfiguration = emailConfiguration;
            _env = env;
            _localizationService = localizationService;

        }

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
        {
            using (var emailClient = new Pop3Client())
            {
                emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, true);

                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);

                List<EmailMessage> emails = new List<EmailMessage>();
                for (int i = 0; i < emailClient.Count && i < maxCount; i++)
                {
                    var message = emailClient.GetMessage(i);
                    var emailMessage = new EmailMessage
                    {
                        Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
                        Subject = message.Subject
                    };
                    emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                    emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                }

                return emails;
            }
        }

        public async Task<bool> SendAsync(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Cc.AddRange(emailMessage.CCAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Bcc.AddRange(emailMessage.BCCAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                try
                {
                    //The last parameter here is to use SSL (Which you should!)
                    emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.StartTls);
                    
                    //Remove any OAuth functionality as we won't be using it. 
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                    emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                    await emailClient.SendAsync(message);

                    emailClient.Disconnect(true);
                   
                }
                catch (Exception ex)
                {                    
                    return false;
                }
            }
            return true;

        }

        // Use our configuration to send the email by using SmtpClient
        public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            EmailMessage emailMessage = new EmailMessage();
            EmailAddress ToAddress = new EmailAddress {Address=email };
            emailMessage.ToAddresses.Add(ToAddress);

            emailMessage.Subject = subject;
            emailMessage.Content = htmlMessage;
            
            EmailAddress SenderAddress = new EmailAddress { Address = _emailConfiguration.SmtpUsername, Name= _emailConfiguration.SenderName };
            emailMessage.FromAddresses.Add(SenderAddress);

           return  await SendAsync(emailMessage);            
            
        }



        public string TemplateForgetPassword(string UserName, string ResetUrl, string ImagePath, string AppUrl)
        {
            string body = string.Empty;
            
            using (StreamReader reader = new StreamReader(_env.ContentRootPath + "\\EmailTemplates\\ForgetPassword.html"))
            {
                body = reader.ReadToEnd();
                
            }
            
            body = body.Replace("{SIDC}", _localizationService.GetLocalized("SIDC"));
            body = body.Replace("{Business Application}", _localizationService.GetLocalized("Business Application"));
            body = body.Replace("{Welcome!}", _localizationService.GetLocalized("Welcome!"));
            body = body.Replace("{Dear }", _localizationService.GetLocalized("Dear "));
            body = body.Replace("{Here's your password!}", _localizationService.GetLocalized("Here's your password!"));
            body = body.Replace("{Please reset your password by click below}", _localizationService.GetLocalized("Please reset your password by click below"));
            body = body.Replace("{CHANGE YOUR PASSWORD}", _localizationService.GetLocalized("CHANGE YOUR PASSWORD"));
            body = body.Replace("{Or go to our website}", _localizationService.GetLocalized("Or go to our website"));          

            body = body.Replace("{UserName}", UserName);
            body = body.Replace("{AppUrl}", AppUrl);
            body = body.Replace("{ResetUrl}", ResetUrl);
            body = body.Replace("{ImagePath}", AppUrl + ImagePath);
            return body;
        }

        //Username
        public string TemplateAccountActivation(string UserName, string Name, string ActivateUrl, string AppUrl)
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(_env.ContentRootPath + "\\EmailTemplates\\AccountActivation.html"))
            {
                body = reader.ReadToEnd();

            }

            body = body.Replace("{SIDC}", _localizationService.GetLocalized("SIDC"));
            body = body.Replace("{Business Application}", _localizationService.GetLocalized("Business Application"));
            body = body.Replace("{Hello}", _localizationService.GetLocalized("Hello"));
            body = body.Replace("{Registration completed}", _localizationService.GetLocalized("Registration completed"));
            body = body.Replace("{Thanks so much for joining Our Team!}", _localizationService.GetLocalized("Thanks so much for joining Our Team!"));
            body = body.Replace("{Your username is: }", _localizationService.GetLocalized("Your username is: "));
            body = body.Replace("{TO FINISH SIGNING UP AND ACTIVATE YOUR ACCOUNT}", _localizationService.GetLocalized("TO FINISH SIGNING UP AND ACTIVATE YOUR ACCOUNT"));
            body = body.Replace("{YOU JUST CLICK BELOW}", _localizationService.GetLocalized("YOU JUST CLICK BELOW"));
            body = body.Replace("{ACTIVATE MY ACCOUNT}", _localizationService.GetLocalized("ACTIVATE MY ACCOUNT"));
            body = body.Replace("{Or go to our website}", _localizationService.GetLocalized("Or go to our website"));
           

            body = body.Replace("{UserName}", UserName);
            body = body.Replace("{Name}", Name);
            body = body.Replace("{ActivationLink}", ActivateUrl);
            body = body.Replace("{ImagePath}", AppUrl + "\\Images\\Emails\\password_icon.jpg");
            body = body.Replace("{AppUrl}", AppUrl);
            return body;
        }
    }


}
