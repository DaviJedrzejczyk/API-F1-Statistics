using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dao.MapConfig
{
    internal class SessionMap : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("F1_SESSION");
            
            builder.HasKey(x => x.SessionKey);

            builder.Property(x => x.SessionKey)
                .ValueGeneratedNever();

            builder.Property(x => x.CircuitKey)
                .IsRequired();

            builder.Property(x => x.CircuitShortName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CountryCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.CountryKey)
                .IsRequired();

            builder.Property(x => x.CountryName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DateStart)
                .IsRequired();

            builder.Property(x => x.DateEnd)
                .IsRequired();

            builder.Property(x => x.GmtOffset)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.IsCancelled)
                .IsRequired();

            builder.Property(x => x.Location)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.MeetingKey)
                .IsRequired();

            builder.Property(x => x.SessionName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.SessionType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Year)
                .IsRequired();

            builder.HasIndex(x => x.MeetingKey);

            builder.HasIndex(x => x.CircuitKey);

            builder.HasIndex(x => x.Year);

        }
    }
}
