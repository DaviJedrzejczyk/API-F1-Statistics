using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class LapMap : IEntityTypeConfiguration<Lap>
    {
        public void Configure(EntityTypeBuilder<Lap> builder)
        {
            builder.ToTable("F1_LAPS");

            builder.HasKey(x => new
            {
                x.MeetingKey,
                x.SessionKey,
                x.DriverNumber,
                x.LapNumber
            });

            builder.Property(x => x.DateStart)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.DurationSector1);

            builder.Property(x => x.DurationSector2);

            builder.Property(x => x.DurationSector3);

            builder.Property(x => x.I1Speed);

            builder.Property(x => x.I2Speed);

            builder.Property(x => x.IsPitOutLap)
                .IsRequired();

            builder.Property(x => x.LapDuration);

            builder.Property(x => x.LapNumber)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.StSpeed);

            builder.Property(x => x.IsFastLap)
                .IsRequired();

            builder.HasMany(x => x.Segments)
                .WithOne(x => x.Lap)
                .HasForeignKey(x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.LapNumber })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
