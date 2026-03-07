using Microsoft.EntityFrameworkCore;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Media> Media { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Media>(entity =>
            {
                entity.ToTable("media");

                entity.HasKey(e => e.MediaId);

                entity.Property(e => e.MediaId)
                    .HasColumnName("media_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserName)
                    .HasColumnName("user_name")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Base64)
                    .HasColumnName("base64")
                    .HasColumnType("text");

                entity.Property(e => e.UrlBucket)
                    .HasColumnName("url_bucket")
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion(
                        v => v.ToString(),
                        v => Enum.Parse<MediaStatus>(v))
                    .IsRequired();

                // Índices
                entity.HasIndex(e => e.UserName)
                    .HasDatabaseName("idx_media_username");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_media_status");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("idx_media_created_at");
            });
        }
    }
}