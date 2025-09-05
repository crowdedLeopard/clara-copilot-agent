# Script to set maximum token lifetime for Clara Copilot Agent

# Check if Microsoft.Graph module is installed
if (!(Get-Module -ListAvailable -Name Microsoft.Graph)) {
    Write-Host "Installing Microsoft Graph PowerShell module..."
    Install-Module Microsoft.Graph -Scope CurrentUser -Force
}

# Import required modules
Import-Module Microsoft.Graph.Authentication
Import-Module Microsoft.Graph.Applications
Import-Module Microsoft.Graph.Identity.SignIns

try {
    # Connect to Microsoft Graph with required permissions
    Write-Host "Connecting to Microsoft Graph..."
    Connect-MgGraph -Scopes "Application.ReadWrite.All", "Policy.ReadWrite.ApplicationConfiguration" -NoWelcome

    # Application ID
    $AppId = "20aed42e-9b90-40b7-94cd-e171bd3e15ac"

    # Create token lifetime policy with maximum values
    Write-Host "Creating token lifetime policy..."
    
    $PolicyDefinition = @{
        definition = @(
            '{"TokenLifetimePolicy":{"Version":1,"AccessTokenLifetime":"24:00:00","RefreshTokenMaxAge":"90.00:00:00","RefreshTokenInactivityTimeout":"90.00:00:00","RefreshTokenMaxAgeMultiFactor":"90.00:00:00"}}'
        )
        displayName = "Clara-Copilot-MaxTokenLifetime"
        isOrganizationDefault = $false
    }

    # Create the policy
    $Policy = New-MgPolicyTokenLifetimePolicy -BodyParameter $PolicyDefinition
    Write-Host "Policy created with ID: $($Policy.Id)"

    # Get the application
    Write-Host "Finding application..."
    $App = Get-MgApplication -Filter "appId eq '$AppId'"
    
    if ($App) {
        Write-Host "Found application: $($App.DisplayName)"
        
        # Assign the policy to the application
        Write-Host "Assigning policy to application..."
        $PolicyReference = @{
            "@odata.id" = "https://graph.microsoft.com/v1.0/policies/tokenLifetimePolicies/$($Policy.Id)"
        }
        
        New-MgApplicationTokenLifetimePolicyByRef -ApplicationId $App.Id -BodyParameter $PolicyReference
        
        Write-Host "✅ SUCCESS: Maximum token lifetime policy applied!" -ForegroundColor Green
        Write-Host ""
        Write-Host "New Token Settings:" -ForegroundColor Yellow
        Write-Host "  • Access Token Lifetime: 24 hours (maximum)" -ForegroundColor Cyan
        Write-Host "  • Refresh Token Max Age: 90 days (maximum)" -ForegroundColor Cyan
        Write-Host "  • Refresh Token Inactivity: 90 days" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "⏰ Changes will take effect in 15-30 minutes" -ForegroundColor Yellow
        Write-Host "🔄 Users may need to re-authenticate once for new tokens" -ForegroundColor Yellow
        
    } else {
        Write-Error "Application with ID $AppId not found!"
    }

} catch {
    Write-Error "Failed to configure token lifetime: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "Alternative: Manual Configuration via Azure Portal" -ForegroundColor Yellow
    Write-Host "1. Go to Azure Portal → Azure Active Directory" -ForegroundColor White
    Write-Host "2. App registrations → Clara Copilot Agent" -ForegroundColor White
    Write-Host "3. Token configuration → Configure token lifetime" -ForegroundColor White
} finally {
    # Disconnect from Microsoft Graph
    Disconnect-MgGraph -ErrorAction SilentlyContinue
}
