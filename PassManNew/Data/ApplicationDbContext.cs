using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PassManNew.Models;

namespace PassManNew.Data
{
    public class ApplicationDbContext  : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder
            //.Query<TeamViewModel>().ToView("vw_MasterList");

            modelBuilder.Entity<Link>()
             .HasOne(p => p.Web)
             .WithMany(b => b.Links)
             .HasForeignKey(p => p.WebId);



            modelBuilder.Entity<DocPermission>()
                .HasKey(o => new { o.DocId, o.PersonId });

            modelBuilder.Entity<LinkPermission>()
            .HasKey(o => new { o.LinkId, o.PersonId });


        }


        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Dept> Depts { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Link> Links { get; set; }

        public virtual DbSet<Web> Webs { get; set; }
        public virtual DbSet<LinkPermission> LinkPermissions { get; set; }

        public virtual DbSet<Log> Logs { get; set; }

        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Doc> Docs { get; set; }
        public virtual DbSet<DocPermission> DocPermissions { get; set; }
    }
}
