using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Notifications.Models;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(n => n.Title)
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnType("varchar(1000)")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnType("varchar(50)")
            .IsRequired()
            .HasDefaultValue(NotificationStatusEnum.Unread);

        builder.Property(n => n.RelatedEntityType)
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(n => n.ActionUrl)
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.Property(n => n.ReadAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(n => n.ReadByUserId)
            .HasColumnType("varchar(450)")
            .IsRequired(false);

        builder.HasOne(n => n.RelatedPerson)
            .WithMany()
            .HasForeignKey(n => n.RelatedPersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(n => n.ReadByUser)
            .WithMany()
            .HasForeignKey(n => n.ReadByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.RelatedPersonId);
        builder.HasIndex(n => n.CreatedAt);
    }
}
