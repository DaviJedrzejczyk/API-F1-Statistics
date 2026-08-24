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
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.HeadshotUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.NameAcronym)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.TeamColour)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.TeamName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.DriverKey);

            builder.HasIndex(x => x.MeetingKey);

            builder.HasIndex(x => x.SessionKey);
        }
    }
}
