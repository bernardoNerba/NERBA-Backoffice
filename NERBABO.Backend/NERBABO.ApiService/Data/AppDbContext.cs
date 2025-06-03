using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Data.Configurations;

namespace NERBABO.ApiService.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Person> People { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<GeneralInfo> GeneralInfo { get; set; } // only 1 instance
    public DbSet<Frame> Frames { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
    {
    }

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
    }

}
