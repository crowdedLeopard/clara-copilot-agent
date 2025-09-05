# Schema Fixes Summary - September 5, 2025 (CRITICAL UPDATES)

## Issues Fixed for Power Platform Connector

### 🚨 CRITICAL: License Count Endpoint Schema Mismatch
**Problem**: Power Platform connector was expecting specific property names and types that didn't match the API response.

**Power Platform Expected Schema**:
- Property names: `totalLicenses`, `usedLicenses`, `availableLicenses` (lowercase)
- Property types: `integer` (without int32 format specifier)

**Fix Applied**:
- Updated C# model with JsonPropertyName attributes to serialize correctly:
  - `TotalLicenses` → serializes as `"totalLicenses"`
  - `AssignedLicenses` → serializes as `"usedLicenses"`
  - `AvailableLicenses` → serializes as `"availableLicenses"`
- Updated Swagger spec to match Power Platform expectations
- Removed `format: int32` which was causing type mismatch errors

**Files Modified**:
- `src/Clara.API/Models/LicenseCountsDto.cs` - Added JsonPropertyName attributes
- `clara-swagger-2.0-corrected.yaml` - Updated schema to match Power Platform format

### Current API Schema (CORRECTED)

### License Count Response
```json
{
  "totalLicenses": 100,
  "usedLicenses": 75,
  "availableLicenses": 25
}
```
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
