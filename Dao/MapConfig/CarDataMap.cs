using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    public class CarDataMap : IEntityTypeConfiguration<CarData>
    {
        public void Configure(EntityTypeBuilder<CarData> builder)
        {
            builder.ToTable("F1_CARDATAS");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

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

            builder.HasIndex(x => x.Id);

            builder.HasIndex(x => x.SessionKey);

            builder.HasIndex(x => x.MeetingKey);

            builder.HasIndex(x => x.DriverNumber);
        }
    }
}
