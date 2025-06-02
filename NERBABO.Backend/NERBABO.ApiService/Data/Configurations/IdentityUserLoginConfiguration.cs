
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NERBABO.ApiService.Data.Configurations;

public class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    {
        builder.ToTable("UserLogins");
        builder.Property(l => l.LoginProvider)
                .HasMaxLength(50);

        builder.Property(l => l.ProviderKey)
            .HasMaxLength(100);

        builder.Property(l => l.ProviderDisplayName)
            .HasMaxLength(100);
    }
}
