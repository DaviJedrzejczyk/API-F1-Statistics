using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    public class CarDataMap : IEntityTypeConfiguration<CarData>
    {
        public void Configure(EntityTypeBuilder<CarData> builder)
        {
            builder.ToTable("F1_CARDATAS");

            builder.HasKey(x => new
            {
                x.SessionKey,
                x.DriverNumber,
                x.Date
            });

            builder.Property(x => x.Brake)
                .IsRequired();

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.Drs)
                .IsRequired(false);

            builder.Property(x => x.MeetingKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.Gear)
                .IsRequired();

            builder.Property(x => x.Rpm)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.Speed)
                .IsRequired();

            builder.Property(x => x.Throttle)
                .IsRequired();
        }
    }
}
