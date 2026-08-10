using Azure.Identity;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Data;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Pipeline;
using LearningHub.UserMigrationService.Repositories;
using LearningHub.UserMigrationService.Services;
using LearningHub.UserMigrationService.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine("BEFORE CONFIGURATION");

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

builder.Services.AddHostedService<MigrationWorker>();

// ------------------------------------------------------
// Build & Run
// ------------------------------------------------------

Console.WriteLine("Before Build");

var host = builder.Build();

Console.WriteLine("After Build");

await host.RunAsync();