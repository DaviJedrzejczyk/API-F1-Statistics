using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class DriverMap : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("F1_DRIVERS");

            builder.HasKey(x => x.DriverKey);

            builder.Property(x => x.DriverKey).UseIdentityColumn().ValueGeneratedOnAdd();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.BroadcastName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.FirstName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.FullName)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.HeadshotUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.LastName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.MeetingKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.NameAcronym)
                .IsRequired(false)
                .HasMaxLength(3);

            builder.Property(x => x.SessionKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.TeamColour)
                .IsRequired(false)
                .HasMaxLength(10);

            builder.Property(x => x.TeamName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.HasIndex(x => x.DriverKey);

            builder.HasIndex(x => x.MeetingKey);

            builder.HasIndex(x => x.SessionKey);
        }
    }
}
