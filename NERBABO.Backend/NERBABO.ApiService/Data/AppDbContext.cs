using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data.Configurations;

namespace NERBABO.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Person> People { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<GeneralInfo> GeneralInfo { get; set; } // only 1 instance
    public DbSet<Frame> Frames { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseAction> Actions { get; set; }
    public DbSet<TeacherModuleAction> TeacherModuleActions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new IdentityUserLoginConfiguration());
        builder.ApplyConfiguration(new IdentityUserClaimConfiguration());
        builder.ApplyConfiguration(new IdentityUserTokenConfiguration());
        builder.ApplyConfiguration(new IdentityRoleConfiguration());
        builder.ApplyConfiguration(new PersonConfiguration());
        builder.ApplyConfiguration(new GeneralInfoConfiguration());
        builder.ApplyConfiguration(new TaxConfiguration());
        builder.ApplyConfiguration(new FrameConfiguration());
        builder.ApplyConfiguration(new TeacherConfiguration());
        builder.ApplyConfiguration(new CompanyConfiguration());
        builder.ApplyConfiguration(new StudentConfiguration());
        builder.ApplyConfiguration(new ModuleConfiguration());
        builder.ApplyConfiguration(new CourseActionConfiguration());
        builder.ApplyConfiguration(new TeacherModuleActionConfiguration());

    }
}
