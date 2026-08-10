using LearningHub.UserMigrationService.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LearningHub.UserMigrationService.Data;

public class UserMigrationDbContext : DbContext
{
    public UserMigrationDbContext(DbContextOptions<UserMigrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MigrationRun> MigrationRuns => Set<MigrationRun>();

    public DbSet<MigrationStepRun> MigrationStepRuns => Set<MigrationStepRun>();

    public DbSet<MigrationLog> MigrationLogs => Set<MigrationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("migration");

        modelBuilder.Entity<MigrationRun>()
            .HasKey(x => x.MigrationRunId);

        modelBuilder.Entity<MigrationStepRun>()
            .HasKey(x => x.MigrationStepRunId);

        modelBuilder.Entity<MigrationLog>()
            .HasKey(x => x.MigrationLogId);

        modelBuilder.Entity<MigrationStepRun>()
            .HasOne<MigrationRun>()
            .WithMany()
            .HasForeignKey(x => x.MigrationRunId);

        modelBuilder.Entity<MigrationLog>()
            .HasOne<MigrationRun>()
            .WithMany()
            .HasForeignKey(x => x.MigrationRunId);

        modelBuilder.Entity<MigrationLog>()
            .HasOne<MigrationStepRun>()
            .WithMany()
            .HasForeignKey(x => x.MigrationStepRunId)
            .IsRequired(false);
    }
}