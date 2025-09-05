# Clara Copilot Agent - Power Platform Custom Connector Configuration

## 🎉 UPDATED - September 5, 2025

### ✅ Fixed Issues:
1. **OAuth Redirect URI Mismatch** - Added missing redirect URI to Azure AD app registration
2. **JSON Schema Type Mismatch** - Fixed date field serialization in usage report endpoint  
3. **"undefined" String Issue** - API now properly handles null DateTime values as JSON null
4. **License Count Schema Mismatch** - CRITICAL FIX: Updated both API and Swagger spec to use Power Platform expected schema:
   - Property names: `totalLicenses`, `usedLicenses`, `availableLicenses` (lowercase)
   - Property types: `integer` (removed int32 format that caused type mismatches)
   - API response mapping: `AssignedLicenses` now serializes as `usedLicenses`
5. **Usage Report Parameters** - Corrected Swagger spec to match actual endpoint (only accepts optional `days` parameter)

### 📂 Latest Files:
- **Swagger Spec**: Use `clara-swagger-2.0-corrected.yaml` for import (CRITICAL schema fixes applied)
- **API Version**: 1.0.2 with Power Platform compatible schema

## Quick Import Instructions

### Step 1: Import Swagger Specification
1. Go to [Power Apps](https://make.powerapps.com)
2. Navigate to **Data** > **Custom connectors**
3. Click **+ New custom connector** > **Import an OpenAPI file**
4. **⚠️ IMPORTANT**: Upload the `clara-swagger-2.0-corrected.yaml` file from this directory (latest with schema fixes)

### Step 2: Configure Authentication
After importing, configure these authentication settings in the **Security** tab:

| Setting | Value |
|---------|--------|
| **Authentication Type** | OAuth 2.0 |
| **Identity Provider** | Generic Oauth 2 |
| **Client ID** | `[YOUR_CLIENT_ID]` |
| **Client Secret** | `[YOUR_CLIENT_SECRET]` |
| **Authorization URL** | `https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/authorize` |
| **Token URL** | `https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/token` |
| **Refresh URL** | `https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/token` |
| **Scope** | `api://[YOUR_API_APPLICATION_ID]/access_as_user` |

### Step 3: Test the Connector
1. Go to the **Test** tab
2. Create a new connection using your Azure AD credentials
3. Test each operation to ensure they work correctly

### ⚠️ Troubleshooting Authentication Errors

**If you get 401 "token expired" errors:**

1. **Quick Fix**: Go to **Test** tab → Click **"New connection"** → Re-authenticate
2. **Alternative**: Delete existing connection in **Data** → **Connections**, then create new one
3. **Root Cause**: OAuth tokens expire (normal security behavior)
4. **See**: `AUTHENTICATION_TROUBLESHOOTING.md` for detailed steps

### Step 4: Use in Copilot Studio
1. Go to [Copilot Studio](https://copilotstudio.microsoft.com)
2. Create or edit a copilot
3. Go to **Settings** > **Generative AI** > **Actions**
4. Add actions from your Clara custom connector

## Azure AD Application Details

### Clara API Application
- **Display Name**: Clara Copilot Agent - API
- **Application ID**: `20aed42e-9b90-40b7-94cd-e171bd3e15ac`
- **Object ID**: `b893833f-d636-4547-861b-8998ebe75954`
- **Tenant ID**: `c87f36f7-fc65-453c-9019-0d724f21bc42`
- **Application ID URI**: `api://20aed42e-9b90-40b7-94cd-e171bd3e15ac`

### Copilot Studio Client Application
- **Display Name**: Clara Copilot Agent - Copilot Studio
- **Application ID**: `[YOUR_CLIENT_ID]`
- **Object ID**: `[YOUR_OBJECT_ID]`
- **Tenant ID**: `[YOUR_TENANT_ID]`
- **Client Secret**: `[YOUR_CLIENT_SECRET]`

## API Endpoints Summary

### License Management
- **GET** `/api/copilot/license-counts` - Get license availability counts
- **POST** `/api/copilot/assign-license-by-email/{userEmail}` - Assign license to user
- **POST** `/api/copilot/remove-license-by-email/{userEmail}` - Remove license from user

### Usage Analytics
- **GET** `/api/copilot/usage-report?days={days}` - Get inactive users report

### Group Management
- **POST** `/api/copilot/add-user-to-group/{userEmail}` - Add user to Copilot group
- **POST** `/api/copilot/remove-user-from-group/{userEmail}` - Remove user from group

## Sample Copilot Studio Conversation Topics

### Topic: License Status Check
**Trigger Phrases**: "license status", "how many licenses", "copilot availability"
**Action**: GetLicenseCounts
**Response**: "We have {totalLicenses} total Copilot licenses. {usedLicenses} are currently assigned and {availableLicenses} are available for new users."

### Topic: Find Inactive Users
**Trigger Phrases**: "inactive users", "unused licenses", "who isn't using copilot"
**Action**: GetUsageReport
**Response**: "I found {totalInactiveUsers} users who haven't used Copilot in the last {reportPeriodDays} days. Would you like me to list them or help optimize license allocation?"

### Topic: Assign License
**Trigger Phrases**: "assign license", "give copilot access", "add user"
**User Input**: Email address
**Action**: AssignLicenseByEmail
**Response**: "I've successfully assigned a Copilot license to {userEmail}. They should have access within a few minutes."

### Topic: Remove License
**Trigger Phrases**: "remove license", "revoke access", "take away copilot"
**User Input**: Email address
**Action**: RemoveLicenseByEmail
**Response**: "I've successfully removed the Copilot license from {userEmail}. The license is now available for reassignment."

### Topic: Group Management
**Trigger Phrases**: "add to group", "manage group access"
**User Input**: Email address
**Action**: AddUserToGroup or RemoveUserFromGroup
**Response**: "I've successfully updated {userEmail}'s group membership."

## Troubleshooting

### Common Issues
1. **Authentication Errors**: Ensure the client secret hasn't expired and all URLs are correct
2. **Permission Errors**: Verify the Azure AD app has the required Microsoft Graph permissions
3. **API Errors**: Check that the Clara API service is running and accessible

### Required Azure AD Permissions
The Clara API app registration should have these Microsoft Graph permissions:
- `User.Read.All` (Application)
- `Group.ReadWrite.All` (Application)
- `Organization.Read.All` (Application)
- `Directory.Read.All` (Application)

### Support
For issues with the Clara API itself, refer to the [GitHub repository](https://github.com/luishdemetrio/clara-copilot-agent).
