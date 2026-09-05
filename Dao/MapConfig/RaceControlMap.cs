using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class RaceControlMap : IEntityTypeConfiguration<RaceControl>
    {
        public void Configure(EntityTypeBuilder<RaceControl> builder)
        {
            builder.ToTable("F1_RACE_CONTROLS");

            builder.HasKey(x => new
            {
                x.Date,
                x.SessionKey,
                x.MeetingKey
            });

            builder.Property(x => x.Category)
            .HasMaxLength(50)
            .IsRequired(false);

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired(false);

            builder.Property(x => x.Flag)
                .HasMaxLength(25)
                .IsRequired(false);

            builder.Property(x => x.LapNumber)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.QualifyingPhase)
                .IsRequired(false);

            builder.Property(x => x.Scope)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.Sector)
                .IsRequired(false);

            builder.Property(x => x.SessionKey)
                .IsRequired();
        }
    }
}
