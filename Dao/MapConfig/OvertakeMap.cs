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
    internal class OvertakeMap : IEntityTypeConfiguration<Overtake>
    {
        public void Configure(EntityTypeBuilder<Overtake> builder)
        {
            builder.ToTable("F1_OVERTAKES");

            builder.HasKey(c => new
            {
                c.MeetingKey,
                c.SessionKey,
                c.OvertakingDriverNumber,
                c.OvertakedDriverNumber,
                c.Date,
                c.Position
            });

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionKey)
                .IsRequired();

            builder.Property(x => x.OvertakedDriverNumber)
                .IsRequired();

            builder.Property(x => x.OvertakingDriverNumber)
                .IsRequired();

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.Position)
                .IsRequired();
        }
    }
}
