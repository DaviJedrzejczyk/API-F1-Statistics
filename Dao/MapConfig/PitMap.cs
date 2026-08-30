using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class PitMap : IEntityTypeConfiguration<Pit>
    {
        public void Configure(EntityTypeBuilder<Pit> builder)
        {
            builder.ToTable("F1_PITS");

            builder.HasKey(x => new
            {
                x.Date,
                x.DriverNumber,
                x.SessionKey,
                x.MeetingKey

            });

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.LaneDuration)
                .IsRequired();

            builder.Property(x => x.LapNumber)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.PitDuration)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.StopDuration)
                .IsRequired();

        }
    }
}