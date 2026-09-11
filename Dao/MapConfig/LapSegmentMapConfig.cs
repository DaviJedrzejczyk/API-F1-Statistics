using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class LapSegmentMapConfig : IEntityTypeConfiguration<LapSegment>
    {
        public void Configure(EntityTypeBuilder<LapSegment> builder)
        {
            builder.ToTable("F1_LAPS_SEGMENTS");

            builder.HasKey(x => new
            {
                x.MeetingKey,
                x.SessionKey,
                x.DriverNumber,
                x.LapNumber,
                x.Sector,
                x.SegmentIndex
            });

            builder.Property(x => x.Sector)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.SegmentStatus)
                .IsRequired(false);

            builder.Property(x => x.SegmentIndex)
                .IsRequired();
        }
    }
}
