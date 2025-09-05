# Schema Fixes Summary - January 15, 2025

## Issues Fixed for Power Platform Connector

### 1. License Count Endpoint Schema Mismatch
**Problem**: Power Platform connector was expecting different property names than what the API was returning.

**Fix Applied**:
- Updated Swagger spec property names to match C# model:
  - `totalLicenses` → `TotalLicenses` (int)
  - `assignedLicenses` → `AssignedLicenses` (int) 
  - `availableLicenses` → `AvailableLicenses` (int)

**Files Modified**:
- `clara-swagger-2.0-corrected.yaml` - Updated LicenseCountsDto definition

### 2. Usage Report Endpoint Parameter Mismatch
**Problem**: Swagger spec defined required `startDate` and `endDate` parameters, but actual API only accepts optional `days` parameter.

**Fix Applied**:
- Removed incorrect `startDate` and `endDate` parameters
- Kept only the `days` parameter (optional, integer, default: 30)
- Matches actual C# controller implementation: `GetM365CopilotUsageReport(int? days = null)`

**Files Modified**:
- `clara-swagger-2.0-corrected.yaml` - Updated /api/copilot/usage-report parameters

### 3. DateTime Serialization
**Previous Fix** (already applied): 
- API properly handles null DateTime values as JSON null instead of "undefined" strings
- C# model uses nullable DateTime properties (`DateTime?`)

## Current API Schema

### License Count Response
```json
{
  "TotalLicenses": 100,
  "AssignedLicenses": 75,
  "AvailableLicenses": 25
}
```

### Usage Report Response
```json
[
  {
    "userId": "12345678-1234-1234-1234-123456789abc",
    "userDisplayName": "John Doe", 
    "userPrincipalName": "john.doe@contoso.com",
    "userDepartment": "IT",
    "lastActivityDate": "2025-01-15T10:30:00.0000000+00:00",
    "copilotChatLastActivityDate": "2025-01-15T10:30:00.0000000+00:00",
    "microsoftTeamsCopilotLastActivityDate": null,
    "wordCopilotLastActivityDate": null,
    "excelCopilotLastActivityDate": null,
    "powerPointCopilotLastActivityDate": null,
    "outlookCopilotLastActivityDate": null,
    "oneNoteCopilotLastActivityDate": null,
    "loopCopilotLastActivityDate": null
  }
]
```

## API Endpoint URLs
- **Base URL**: `https://clara-copilot-api-cpg7bqbecffpe8ha.uksouth-01.azurewebsites.net`
- **License Count**: `GET /api/copilot/license-counts`
- **Usage Report**: `GET /api/copilot/usage-report?days=30`

## Next Steps for Power Platform
1. Re-import the corrected Swagger spec (`clara-swagger-2.0-corrected.yaml`)
2. Test both endpoints in the custom connector
3. Verify schema parsing works without errors
4. Update any Power Apps or Power Automate flows that use these endpoints

## Authentication
- All endpoints require OAuth2 authentication
- Use the Azure AD app registration configured for the Power Platform connector
- Redirect URI: `https://global.consent.azure-apim.net/redirect`
