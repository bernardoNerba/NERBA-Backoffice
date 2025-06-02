using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NERBABO.ApiService.Data.Configurations;

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        builder.ToTable("UserClaims");

        builder.Property(c => c.ClaimType)
                .HasMaxLength(100);

        builder.Property(c => c.ClaimValue)
            .HasMaxLength(500);
    }
}
