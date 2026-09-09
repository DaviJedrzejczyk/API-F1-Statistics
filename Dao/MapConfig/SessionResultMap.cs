using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.MapConfig
{
    internal class SessionResultMap : IEntityTypeConfiguration<SessionResult>
    {
        public void Configure(EntityTypeBuilder<SessionResult> builder)
        {
            builder.ToTable("F1_SESSION_RESULTS");

            builder.HasKey(x => new
            {
                x.SessionKey,
                x.DriverNumber
            });

            builder.Property(x => x.Dnf)
                .IsRequired();

            builder.Property(x => x.Dns)
                .IsRequired();

            builder.Property(x => x.Dsq)
                .IsRequired();

            builder.Property(x => x.DriverNumber)
                .IsRequired();

            builder.Property(x => x.Duration)
                .IsRequired();

            builder.Property(x => x.GapToLeader)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.NumberOfLaps)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.Position)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.Points)
                .IsRequired();

            builder.Property(x => x.IsQualy)
                .IsRequired();
        }
    }
}
