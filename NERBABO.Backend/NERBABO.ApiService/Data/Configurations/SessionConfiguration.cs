using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Sessions.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ModuleTeachingId)
                .IsRequired();

            builder.Property(s => s.Weekday)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.ScheduledDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(s => s.Start)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(s => s.DurationHours)
                .HasColumnType("float")
                .IsRequired();

            builder.HasOne(s => s.ModuleTeaching)
                .WithMany(mt => mt.Sessions)
                .HasForeignKey(s => s.ModuleTeachingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(s => s.End);
            builder.Ignore(s => s.Time);
        }
    }
}