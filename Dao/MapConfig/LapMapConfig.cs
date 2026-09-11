using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dao.MapConfig
{
    internal class LapMapConfig : IEntityTypeConfiguration<Lap>
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

            builder.Property(x => x.DurationSector1)
                .IsRequired();

            builder.Property(x => x.DurationSector2)
                .IsRequired();

            builder.Property(x => x.DurationSector3)
                .IsRequired();

            builder.Property(x => x.I1Speed)
                .IsRequired();

            builder.Property(x => x.I2Speed)
                .IsRequired();

            builder.Property(x => x.IsPitOutLap)
                .IsRequired();

            builder.Property(x => x.LapDuration)
                .IsRequired();

            builder.Property(x => x.LapNumber)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.StSpeed)
                .IsRequired();

            builder.HasMany(x => x.Segments)
                .WithOne(x => x.Lap)
                .HasForeignKey(x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.LapNumber })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
