
using Microsoft.EntityFrameworkCore;
using PassManNew.Data;


namespace PassManNew.Models
{
    public static class SeedData
    {
        
        public static async System.Threading.Tasks.Task InitializeAsync(IServiceProvider serviceProvider)
        {
             using(var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
            
                // Look for any movies.
                //if (!context.Shifts.Any())
                //{
                //    context.Shifts.AddRange(
                //    new Shift
                //    {
                //        Name = "Default Shift",
                //        StartTime = TimeSpan.Parse("08:00"),
                //        EndTime = TimeSpan.Parse("17:00"),
                //        State = Shared.State.Active
                //    });
                //    await context.SaveChangesAsync();
                //}

                // Look for any movies.
                //if (!context.Settings.Any())
                //{
                //    context.Settings.Add(
                //    new Setting
                //    {
                //        AccountantId = "",
                //        GMId = ""                    
                //    });
                //    await context.SaveChangesAsync();
                //}

                //var conn = context.Database.GetDbConnection();
                //if (conn.State.Equals(ConnectionState.Closed)) await conn.OpenAsync();
                //using (var command = conn.CreateCommand())
                //{
                //    command.CommandText = @"
                //    SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'vw_MasterList'";
                //    var exists = await command.ExecuteScalarAsync() != null;


                //    if (!exists)
                //    {
                //        context.Database.ExecuteSqlCommand(
                //         @"
                //        Create View vw_MasterList as
                //        Select so, Id,PersonName,Title,PhoneNumber,Email, RegionName, RegionalManagerName, RegionalManagerId, AreaName,
                //        AreaManagerName, AreaManagerId, SupervisorName, SupervisorPositionName, 
                //        SupervisorId, LocationId, LocationName, City, ShiftName, Shift
                //        from (

                //        Select '1' so,u.Id,PersonName,'Regional Manager' Title,PhoneNumber,Email,r.Name RegionName,'-' RegionalManagerName,
                //        '-' RegionalManagerId,
                //            '-' AreaName,'-' AreaManagerName,'-' AreaManagerId, 
                //            '-' SupervisorName, '-' SupervisorPositionName,'-' SupervisorId,
                //         null LocationId, '-' LocationName, '-' City,'-' ShiftName,'-'  Shift
                //        from AspNetUsers u inner join Region r on u.id = r.RegionalManagerId 

                //        where r.State = 1  ) region

                //        Union All

                //        Select '2' so,u.Id,u.PersonName,'Area Manager' Title,u.PhoneNumber,u.Email,
                //        r.Name RegionName,ru.PersonName RegionalManagerName,r.RegionalManagerId RegionalManagerId,
                //            a.Name AreaName, '-' AreaManagerName,'-' AreaManagerId,
                //        '-' SupervisorName, '-' SupervisorPositionName,'-' SupervisorId,
                //        null LocationId, '-' LocationName, '-' City,'-' ShiftName,'-'  Shift
                //        from AspNetUsers u inner join Area A on u.id = a.AreaManagerID
                //        inner join Region r on r.Id = a.RegionId 
                //        inner join AspnetUsers ru on ru.id = r.RegionalManagerId
                //        where a.State = 1  

                //        Union All

                //        Select '3' so,u.Id,u.PersonName,'Supervisor' Title,u.PhoneNumber,u.Email,
                //        r.Name RegionName,ru.PersonName RegionalManagerName,r.RegionalManagerId RegionalManagerId,
                //            a.Name AreaName,au.PersonName AreaManagerName, a.AreaManagerID AreaManagerId,
                //            '-' SupervisorName, s.Name SupervisorPositionName, '-' SupervisorId,
                //         null LocationId,   '-' LocationName, '-' City,
                //         sh.Name ShiftName,convert(varchar(5),sh.StartTime) + ' - ' + convert(varchar(5),sh.EndTime)  Shift
                //        from AspNetUsers u inner join Supervisor s on u.id = s.SupervisorUserId
                //        inner join Shift sh on sh.id = s.ShiftId
                //        inner join Area a on a.Id = s.AreaId
                //        inner join AspNetUsers au on au.Id = a.AreaManagerID
                //        inner join Region r on r.Id = a.RegionId 
                //        inner join AspnetUsers ru on ru.id = r.RegionalManagerId
                //        where s.State = 1

                //        Union All

                //        Select '4' so,isnull(u.Id,''),isnull(u.PersonName,'Vacant'),'SalesMan' Title,isnull(u.PhoneNumber,''),
                //        isnull(u.Email,''),r.Name RegionName,
                //        ru.PersonName RegionalManagerName,r.RegionalManagerId RegionalManagerId,
                //        a.Name AreaName,au.PersonName AreaManagerName,a.AreaManagerID AreaManagerId,
                //        su.PersonName SupervisorName, s.Name SupervisorPositionName, s.SupervisorUserId SupervisorId, 
                //        loc.Id LocationId, loc.Name LocationName, loc.City City, 
                //        sh.Name ShiftName,convert(varchar(5),sh.StartTime) + ' - ' + convert(varchar(5),sh.EndTime)  Shift
                //        from SalesMan sm						 
                //        Left join AspNetUsers u on sm.SaleManUserId = u.id
                //        inner join location loc on sm.LocationId = loc.Id
                //        inner join Supervisor s on sm.SupervisorId = s.id
                //        inner join AspNetUsers su on su.Id = s.SupervisorUserId
                //        inner join Shift sh on sh.id = s.ShiftId
                //        inner join Area a on a.Id = s.AreaId
                //        inner join AspNetUsers au on au.Id = a.AreaManagerID
                //        inner join Region r on r.Id = a.RegionId 
                //        inner join AspnetUsers ru on ru.id = r.RegionalManagerId
                //        where sm.State = 1"

                //        );

                //        context.SaveChanges();
                //    }
                //}
            }
        }


    }
}