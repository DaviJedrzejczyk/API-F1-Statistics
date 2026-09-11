using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class MeetingMap : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            builder.ToTable("F1_MEETINGS");

            builder.HasKey(x => x.MeetingKey);

            builder.Property(x => x.MeetingKey)
                .ValueGeneratedNever();

            builder.Property(x => x.CircuitKey)
                .IsRequired();

            builder.Property(x => x.CircuitInfoUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CircuitImage)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CircuitShortName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CircuitType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CountryCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.CountryFlag)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CountryKey)
                .IsRequired();

            builder.Property(x => x.CountryName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DateStart)
                .IsRequired();

            builder.Property(x => x.DateEnd)
                .IsRequired();

            builder.Property(x => x.GmtOffset)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.IsCancelled)
                .IsRequired();

            builder.Property(x => x.Location)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.MeetingName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.MeetingOfficialName)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Year)
                .IsRequired();

            builder.HasIndex(x => x.CircuitKey);

            builder.HasIndex(x => x.Year);
        }
    }
}
