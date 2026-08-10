using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient? _secretClient;
    private readonly ILogger<KeyVaultService> _logger;

    public KeyVaultService(
        IConfiguration configuration,
        ILogger<KeyVaultService> logger)
    {
        _logger = logger;

        var vaultUrl = configuration["KeyVault:VaultUrl"];

        if (!string.IsNullOrWhiteSpace(vaultUrl))
        {
            _secretClient = new SecretClient(
                new Uri(vaultUrl),
                new DefaultAzureCredential());

            _logger.LogInformation("Connected to Key Vault {VaultUrl}", vaultUrl);
        }
        else
        {
            _logger.LogWarning("Key Vault URL not configured.");
        }
    }

    public async Task<string?> GetSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default)
    {
        if (_secretClient == null)
        {
            return null;
        }

        try
        {
            KeyVaultSecret secret =
                await _secretClient.GetSecretAsync(
                    secretName,
                    cancellationToken: cancellationToken);

            return secret.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to retrieve secret {SecretName}",
                secretName);

            return null;
        }
    }
}