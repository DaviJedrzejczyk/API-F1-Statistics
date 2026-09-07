using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class StintMap : IEntityTypeConfiguration<Stint>
    {
        public void Configure(EntityTypeBuilder<Stint> builder)
        {
            builder.ToTable("F1_STINTS");
            
            builder.HasKey(x => new
            {
                x.MeetingKey,
                x.SessionKey,
                x.DriverNumber,
                x.StintNumber
            });

            builder.Property(x => x.Compound)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StintNumber) 
                .IsRequired();

            builder.Property(x => x.LapStart)
                .IsRequired(false);
            
            builder.Property(x => x.LapEnd)
                .IsRequired(false);

            builder.Property(x => x.TyreAgeAtStart)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();
        }
    }
}
