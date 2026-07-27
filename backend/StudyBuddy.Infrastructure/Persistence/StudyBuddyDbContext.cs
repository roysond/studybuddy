using Microsoft.EntityFrameworkCore;
using StudyBuddy.Domain.Entities;

namespace StudyBuddy.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for StudyBuddy PostgreSQL persistence.
/// </summary>
public sealed class StudyBuddyDbContext : DbContext
{
    public StudyBuddyDbContext(DbContextOptions<StudyBuddyDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<StudyMaterial>(entity =>
        {
            entity.ToTable("study_materials");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
