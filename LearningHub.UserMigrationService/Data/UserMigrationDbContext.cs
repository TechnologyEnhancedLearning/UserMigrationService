using LearningHub.UserMigrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningHub.UserMigrationService.Data;

public class UserMigrationDbContext : DbContext
{
    public UserMigrationDbContext(
        DbContextOptions<UserMigrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MigrationRun> MigrationRuns { get; set; }
    public DbSet<MigrationStepRun> MigrationStepRuns { get; set; }
    public DbSet<MigrationLog> MigrationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MigrationRun
        modelBuilder.Entity<MigrationRun>(entity =>
        {
            entity.ToTable("MigrationRun", "migrations");

            entity.HasKey(x => x.MigrationRunId);

            entity.Property(x => x.CreatedUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAdd();
        });

        // MigrationStepRun
        modelBuilder.Entity<MigrationStepRun>(entity =>
        {
            entity.ToTable("MigrationStepRun", "migrations");

            entity.HasKey(x => x.MigrationStepRunId);

            entity.Property(x => x.CreatedUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAdd();
        });

        // MigrationLog
        modelBuilder.Entity<MigrationLog>(entity =>
        {
            entity.ToTable("MigrationLog", "migrations");

            entity.HasKey(x => x.MigrationLogId);

            entity.HasOne<MigrationRun>()
                .WithMany()
                .HasForeignKey(x => x.MigrationRunId);

            entity.HasOne<MigrationStepRun>()
                .WithMany()
                .HasForeignKey(x => x.MigrationStepRunId)
                .IsRequired(false);

            entity.Property(x => x.CreatedUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
        });
    }
}