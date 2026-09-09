using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class SessionResultQualifyMap : IEntityTypeConfiguration<SessionResultQualify>
    {
        public void Configure(EntityTypeBuilder<SessionResultQualify> builder)
        {
            builder.ToTable("F1_SESSION_RESULTS_QUALIFYINGS");

            builder.HasKey(x => new
            {
                x.MeetingKey,
                x.SessionKey,
                x.DriverNumber,
                x.QualifyingPhase
            });

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.MeetingKey) 
                .IsRequired();

            builder.Property(x => x.QualifyingPhase)
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.Duration)
                .IsRequired();

            builder.Property(x => x.GapToLeader)
                .IsRequired();
        }
    }
}
