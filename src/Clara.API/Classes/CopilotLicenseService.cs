using Clara.API.Interfaces;
using Clara.API.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Clara.API.Classes;

public class CopilotLicenseService : ICopilotLicenseService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IConfiguration _config;
    private readonly string _copilotSkuId;

    public CopilotLicenseService(GraphServiceClient graphClient, IConfiguration config)
    {
        _graphClient = graphClient;
        _config = config;
        _copilotSkuId = _config["CopilotSkuId"]!;
    }

    public async Task<bool> AssignLicenseByEmailAsync(string userEmail)
    {
        // Sanitize the email by removing invisible characters
        userEmail = SanitizeEmail(userEmail);
        
        // Find user by userPrincipalName (email)
        var users = await _graphClient.Users
            .GetAsync(config => 
            {
                config.QueryParameters.Filter = $"userPrincipalName eq '{userEmail}'";
                config.QueryParameters.Select = new[] { "id", "userPrincipalName" };
            });

        var user = users?.Value?.FirstOrDefault();
        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        return await AssignLicenseByIdAsync(user.Id);
    }

    public async Task<bool> RemoveLicenseByEmailAsync(string userEmail)
    {
        // Sanitize the email by removing invisible characters
        userEmail = SanitizeEmail(userEmail);
        
        // Find user by userPrincipalName (email)
        var users = await _graphClient.Users
            .GetAsync(config => 
            {
                config.QueryParameters.Filter = $"userPrincipalName eq '{userEmail}'";
                config.QueryParameters.Select = new[] { "id", "userPrincipalName" };
            });

        var user = users?.Value?.FirstOrDefault();
        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        return await RemoveLicenseByIdAsync(user.Id);
    }
    
    private static string SanitizeEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return email;
            
        // Remove invisible characters like zero-width space (\u200B)
        return email.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Trim();
    }

    public async Task<LicenseCountsDto> GetLicenseCountsAsync()
    {
        var skus = await _graphClient.SubscribedSkus.GetAsync();

        var copilotSku = skus?.Value?
            .FirstOrDefault(sku =>
                sku.SkuId.HasValue &&
                sku.SkuId.Value.ToString().Equals(_copilotSkuId, StringComparison.OrdinalIgnoreCase)
            );

        if (copilotSku == null)
            return new LicenseCountsDto();

        var total = copilotSku.PrepaidUnits?.Enabled ?? 0;
        var assigned = copilotSku.ConsumedUnits ?? 0;
        var available = total - assigned;

        return new LicenseCountsDto
        {
            TotalLicenses = total,
            AssignedLicenses = assigned,
            AvailableLicenses = available
        };
    }

    public async Task<bool> AssignLicenseByIdAsync(string userId)
    {
        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>
        {
            new AssignedLicense { SkuId = skuGuid }
        },
            RemoveLicenses = new List<Guid?>()
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return true;
    }

    public async Task<bool> RemoveLicenseByIdAsync(string userId)
    {
        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>(),
            RemoveLicenses = new List<Guid?>() { skuGuid }
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return true;
    }

}
