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
    public DbSet<UserIdToMigrate> UserIdsToMigrate { get; set; }
    public DbSet<OrganisationLocationIdToMigrate> OrganisationLocationIdsToMigrate{ get; set; }
    public DbSet<ValidationIssue> ValidationIssues { get; set; }
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
        modelBuilder.Entity<UserIdToMigrate>(entity =>
        {
            entity.ToTable("UserIdsToMigrate", "migrations");

            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId)
                .ValueGeneratedNever();
        });
        modelBuilder.Entity<OrganisationLocationIdToMigrate>(entity =>
        {
            entity.ToTable(
                "OrganisationLocationIdsToMigrate",
                "migrations");

            entity.HasKey(x => x.LocationId);

            entity.Property(x => x.LocationId)
                .ValueGeneratedNever();
        });
        modelBuilder.Entity<ValidationIssue>(entity =>
        {
            entity.ToTable(
                "ValidationIssues",
                "migrations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.MigrationRunId)
                .IsRequired();

            entity.Property(x => x.TableName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.ValidationType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Severity)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ColumnName)
                .HasMaxLength(128);

            entity.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => x.MigrationRunId);

            entity.HasIndex(x => x.TableName);

            entity.HasIndex(x => x.Severity);

            entity.HasIndex(x => x.ValidationType);

            entity.HasIndex(x => x.StagingRecordId);

            entity.HasIndex(x => x.ElfhRecordId);
        });
    }
}