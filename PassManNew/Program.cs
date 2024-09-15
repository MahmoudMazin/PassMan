using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PassManNew.Controllers;
using PassManNew.Data;
using PassManNew.Models;
using PassManNew.Resources;
using PassManNew.Services.EmailService;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.Add(new ServiceDescriptor(typeof(IMyLog), typeof(MyLog), ServiceLifetime.Scoped));

// Localization......
builder.Services.AddSingleton<LocalizationService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
           .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
           .AddDataAnnotationsLocalization(options =>
           {
               options.DataAnnotationLocalizerProvider = (type, factory) =>
               {
                   var assemblyName = new AssemblyName(typeof(Resource).GetTypeInfo().Assembly.FullName);
                   return factory.Create("Resource", assemblyName.Name);
               };
           });
builder.Services.Configure<RequestLocalizationOptions>(
    options =>
    {
        var supportedCultures = new List<CultureInfo>
            {
                            new CultureInfo("en-US"),
                            new CultureInfo("ar-SA")
            };
        options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    });

// End of Localization......

// DB Log Services
builder.Services.Add(new ServiceDescriptor(typeof(IMyLog), typeof(MyLog), ServiceLifetime.Scoped));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                   builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    //New_Comment
    //.AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddHangfire(configuration =>
       configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

// Add Hangfire services
builder.Services.AddHangfireServer();


builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(480));

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;


    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.User.RequireUniqueEmail = true;

});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;

    // Session TimeOut Settings
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30 Min
    // options.LoginPath = "/Identity/Account/Login";
    options.LoginPath = "/ACount/SignIn";
    options.LogoutPath = $"/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;

});
builder.Services.AddSingleton<IEmailConfiguration, EmailConfiguration>();
builder.Services.AddSingleton<IAppSettings, AppSettings>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddSession();
//services.AddJqueryDataTables();
//builder.Services.AddAutoMapper(typeof(Startup));
builder.Services.AddScoped<IBackgroundJobs, BackgroundJobs>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for PassManNew scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHangfireServer();
app.UseHttpsRedirection();
app.UseStaticFiles();
//var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
//app.UseRequestLocalization(locOptions.Value);
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
});
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
