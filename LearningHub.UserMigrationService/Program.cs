using Azure.Identity;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Data;
using LearningHub.UserMigrationService.Extractors;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Pipeline;
using LearningHub.UserMigrationService.Repositories;
using LearningHub.UserMigrationService.Services;
using LearningHub.UserMigrationService.Transformers;
using LearningHub.UserMigrationService.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
var builder = Host.CreateApplicationBuilder(args);

// ------------------------------------------------------
// Configuration
// ------------------------------------------------------

builder.Configuration
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();
// ------------------------------------------------------
// Azure Key Vault
// ------------------------------------------------------

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration["KeyVault:VaultUrl"]!),
        new DefaultAzureCredential());
}

// ------------------------------------------------------
// Database configuration
// ------------------------------------------------------

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("DatabaseOptions"));

builder.Services.Configure<MigrationOptions>(
    builder.Configuration.GetSection("MigrationOptions"));

builder.Services.AddDbContext<UserMigrationDbContext>((sp, options) =>
{
    var databaseOptions =
        sp.GetRequiredService<IOptions<DatabaseOptions>>();

    options.UseSqlServer(
        databaseOptions.Value.LearningHubConnectionString);
});

// ------------------------------------------------------
// Logging
// ------------------------------------------------------

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ------------------------------------------------------
// Dependency Injection
// ------------------------------------------------------

builder.Services.AddScoped<IMigrationPipeline, MigrationPipeline>();

builder.Services.AddScoped<ILegacyRepository, LegacyRepository>();

builder.Services.AddScoped<ILearningHubRepository, LearningHubRepository>();

builder.Services.AddScoped<IMigrationLogger, MigrationLogger>();

builder.Services.AddScoped<IMigrationRunRepository, MigrationRunRepository>();
builder.Services.AddScoped<IUserMigrationSelectionService,UserMigrationSelectionService>();
builder.Services.AddScoped<IOrganisationMigrationSelectionService,OrganisationMigrationSelectionService>();
builder.Services.AddScoped<IUserExtractor, UserExtractor>();
builder.Services.AddScoped<IUserEmploymentExtractor,UserEmploymentExtractor>();
builder.Services.AddScoped<IUserAdminLocationExtractor,UserAdminLocationExtractor>();
builder.Services.AddScoped<IUserGroupReporterExtractor,UserGroupReporterExtractor>();
builder.Services.AddScoped<IOrganisationExtractor,OrganisationExtractor>();
builder.Services.AddScoped<IProfessionalBodyExtractor, ProfessionalBodyExtractor>();
builder.Services.AddScoped<ISupportingLookupExtractor, SupportingLookupExtractor>();
builder.Services.AddScoped<IProfessionalBodyTransformer,ProfessionalBodyTransformer>();
builder.Services.AddScoped<IOrganisationTransformer,OrganisationTransformer>();
builder.Services.AddScoped<IOrganisationTypeTransformer,OrganisationTypeTransformer>();
builder.Services.AddScoped<IUserTransformer, UserTransformer>();
builder.Services.AddScoped<IUserEmploymentTransformer,UserEmploymentTransformer>();
builder.Services.AddScoped<IUserAdminLocationTransformer,UserAdminLocationTransformer>();
builder.Services.AddScoped<IUserGroupReporterTransformer,UserGroupReporterTransformer>();
builder.Services.AddScoped<IProfessionalBodyMappingRepository,ProfessionalBodyMappingRepository>();
builder.Services.AddScoped<IStagingRepository,StagingRepository>();

builder.Services.AddHostedService<MigrationWorker>();

// ------------------------------------------------------
// Build & Run
// ------------------------------------------------------

var host = builder.Build();

await host.RunAsync();