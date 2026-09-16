using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class LapFastSectorMap : IEntityTypeConfiguration<LapFastSector>
    {
        public void Configure(EntityTypeBuilder<LapFastSector> builder)
        {
            builder.ToTable("F1_LAPS_FAST_SECTORS");
            builder.HasKey(x => new
            {
                x.SessionKey,
                x.DriverNumber,
                x.Sector
            });
            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.DriverName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            
            builder.Property(x => x.Sector)
                .IsRequired();

            builder.Property(x => x.Duration)
                .IsRequired();
        }
    }
}
