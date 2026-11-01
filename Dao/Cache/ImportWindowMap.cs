using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dao.Cache;

internal sealed class ImportWindowMap : IEntityTypeConfiguration<ImportWindow>
{
    public void Configure(EntityTypeBuilder<ImportWindow> builder)
    {
        builder.ToTable("F1_IMPORT_WINDOWS");
        builder.HasKey(x => new { x.Dataset, x.WindowKey });
        builder.Property(x => x.Dataset).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.WindowKey).HasMaxLength(256).IsUnicode(false);
        builder.Property(x => x.CompletedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.IsComplete).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
    }
}
