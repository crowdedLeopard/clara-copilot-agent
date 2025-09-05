# 🔐 Azure AD Token Lifetime Configuration

## Maximum Token Lifetime Settings

### Current Azure AD Limits:
- **Access Token**: Maximum 24 hours
- **Refresh Token**: Maximum 90 days (for web apps)
- **ID Token**: Maximum 24 hours

### PowerShell Script to Set Maximum Token Lifetime

```powershell
# Install Microsoft Graph PowerShell if not already installed
# Install-Module Microsoft.Graph -Scope CurrentUser -Force

# Connect to Microsoft Graph
Connect-MgGraph -Scopes "Application.ReadWrite.All", "Policy.ReadWrite.ApplicationConfiguration"

# Your application ID
$AppId = "20aed42e-9b90-40b7-94cd-e171bd3e15ac"

# Create token lifetime policy with maximum values
$PolicyDefinition = @{
    definition = @(
        @{
            TokenLifetimePolicy = @{
                Version = 1
                AccessTokenLifetime = "24:00:00"      # 24 hours (maximum)
                RefreshTokenMaxAge = "90.00:00:00"   # 90 days (maximum)
                RefreshTokenInactivityTimeout = "90.00:00:00"  # 90 days
                RefreshTokenMaxAgeMultiFactor = "90.00:00:00"  # 90 days
            }
        } | ConvertTo-Json
    )
    displayName = "Clara-Copilot-MaxTokenLifetime"
    isOrganizationDefault = $false
}

# Create the policy
$Policy = New-MgPolicyTokenLifetimePolicy -BodyParameter $PolicyDefinition

# Get the application
$App = Get-MgApplication -Filter "appId eq '$AppId'"

# Assign the policy to the application
New-MgApplicationTokenLifetimePolicyByRef -ApplicationId $App.Id -OdataId "https://graph.microsoft.com/v1.0/policies/tokenLifetimePolicies/$($Policy.Id)"

Write-Host "Token lifetime policy applied successfully!"
Write-Host "New Settings:"
Write-Host "- Access Token: 24 hours"
Write-Host "- Refresh Token: 90 days"
```

### Alternative: Azure CLI Approach

```bash
# Create token lifetime policy JSON
cat > token-policy.json << EOF
{
  "definition": [
    "{\"TokenLifetimePolicy\":{\"Version\":1,\"AccessTokenLifetime\":\"24:00:00\",\"RefreshTokenMaxAge\":\"90.00:00:00\",\"RefreshTokenInactivityTimeout\":\"90.00:00:00\",\"RefreshTokenMaxAgeMultiFactor\":\"90.00:00:00\"}}"
  ],
  "displayName": "Clara-Copilot-MaxTokenLifetime",
  "isOrganizationDefault": false
}
EOF

# Create the policy using Microsoft Graph REST API
az rest --method POST \
  --url "https://graph.microsoft.com/v1.0/policies/tokenLifetimePolicies" \
  --body @token-policy.json \
  --headers "Content-Type=application/json"
```

### Manual Configuration via Azure Portal

1. **Go to Azure Portal** → **Azure Active Directory**
2. **Enterprise Applications** → Find "Clara Copilot Agent"
3. **Token configuration** → **Add optional claim**
4. **Advanced settings** → Set token lifetimes

### Expected Results After Configuration:
- Access tokens will last **24 hours** instead of 1-4 hours
- Refresh tokens will last **90 days**
- Significantly fewer authentication prompts for users
- Better user experience in Power Platform

### Important Notes:
- These are **maximum** values allowed by Azure AD
- Longer tokens = convenience vs. security trade-off
- Consider your organization's security policies
- Changes may take 15-30 minutes to take effect

---
**Status**: Configuration script ready for execution
**Impact**: Will reduce token expiration issues from hourly to daily
