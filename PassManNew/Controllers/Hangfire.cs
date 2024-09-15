using Hangfire;
using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassManNew.Data;
using Microsoft.EntityFrameworkCore;

namespace PassManNew.Controllers
{
    public static class Hangfire
    {
        public static void SetJobs() {

            // */1 every min
            // minutes hours dayofmonth month dayweek
            // https://crontab.guru/examples.html

            //Set attendance status at midnight
            //RecurringJob.AddOrUpdate<IBackgroundJobs>(job => job.SetAttendanceStatus(System.DateTime.Now.ToString("dd-MMM-yyyy"),"",""), Cron.Daily);

            //Create Schedule at 1st of each Month             
            //RecurringJob.AddOrUpdate<IBackgroundJobs>(job => job.CreateSchedule(null, null, null), "0 0 1 * *");          

        }
    }
    public class BackgroundJobs : IBackgroundJobs
    {

        private readonly ApplicationDbContext _context;

        public BackgroundJobs(ApplicationDbContext context)
        {
            _context = context;
        }

        public void CreateSchedule(string StartDate, string EndDate, string jobId)
        {
            // business logic and data persistence here...

            // _context.Database.ExecuteSqlCommand("exec ResetAttendanceStatus @p0", System.DateTime.Now.Date.ToString("dd-MMM-yyyy"));
            // _context.Database.ExecuteSqlCommand("exec dbo.CreateSchedules @p0,@p1,@p2", parameters: new[] { StartDate, EndDate, jobId });

        }

    }


    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.IsInRole("Admin");
        }
    }

    public interface IBackgroundJobs
    {      
       // void CreateSchedule(string StartDate, string EndDate, string jobId);

    }

    
}
