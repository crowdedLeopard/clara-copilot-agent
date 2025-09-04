# Clara Copilot Studio Custom Connector Configuration

## Power Platform Custom Connector Setup

### General Tab
- **Connector name**: Clara Copilot Agent
- **Description**: Connector for Clara M365 Copilot license management
- **Host**: clara-copilot-api-cpg7bqbecffpe8ha.uksouth-01.azurewebsites.net
- **Base URL**: /

### Security Tab
- **Authentication Type**: OAuth 2.0
- **Identity Provider**: Azure Active Directory
- **Client ID**: [YOUR_CLIENT_ID]
- **Client Secret**: [YOUR_CLIENT_SECRET]
- **Authorization URL**: https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/authorize
- **Token URL**: https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/token
- **Refresh URL**: https://login.microsoftonline.com/[YOUR_TENANT_ID]/oauth2/v2.0/token
- **Scope**: api://[YOUR_API_APPLICATION_ID]/access_as_user

### Definition Tab - Actions

#### Action 1: Get License Counts
- **Operation ID**: GetLicenseCounts
- **Summary**: Get Copilot license availability counts
- **Description**: Returns the total, used, and available Copilot licenses
- **Verb**: GET
- **URL**: /api/copilot/license-counts
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json

#### Action 2: Get Usage Report
- **Operation ID**: GetUsageReport
- **Summary**: Get Copilot usage report for inactive users
- **Description**: Returns users who haven't used Copilot in specified days
- **Verb**: GET
- **URL**: /api/copilot/usage-report
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json
- **Parameters**:
  - Name: days
  - Type: integer
  - In: query
  - Required: false
  - Description: Number of days to check for inactivity (default: 30)

#### Action 3: Remove License by Email
- **Operation ID**: RemoveLicenseByEmail
- **Summary**: Remove Copilot license from user by email
- **Description**: Removes a Copilot license from the specified user
- **Verb**: POST
- **URL**: /api/copilot/remove-license-by-email/{userEmail}
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json
- **Parameters**:
  - Name: userEmail
  - Type: string
  - In: path
  - Required: true
  - Description: Email address of the user

#### Action 4: Assign License by Email
- **Operation ID**: AssignLicenseByEmail
- **Summary**: Assign Copilot license to user by email
- **Description**: Assigns a Copilot license to the specified user
- **Verb**: POST
- **URL**: /api/copilot/assign-license-by-email/{userEmail}
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json
- **Parameters**:
  - Name: userEmail
  - Type: string
  - In: path
  - Required: true
  - Description: Email address of the user

#### Action 5: Add User to Group
- **Operation ID**: AddUserToGroup
- **Summary**: Add user to Copilot group
- **Description**: Adds a user to the configured Copilot group
- **Verb**: POST
- **URL**: /api/copilot/add-user-to-group/{userEmail}
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json
- **Parameters**:
  - Name: userEmail
  - Type: string
  - In: path
  - Required: true
  - Description: Email address of the user

#### Action 6: Remove User from Group
- **Operation ID**: RemoveUserFromGroup
- **Summary**: Remove user from Copilot group
- **Description**: Removes a user from the configured Copilot group
- **Verb**: POST
- **URL**: /api/copilot/remove-user-from-group/{userEmail}
- **Headers**: 
  - Authorization: Bearer {token}
  - Content-Type: application/json
- **Parameters**:
  - Name: userEmail
  - Type: string
  - In: path
  - Required: true
  - Description: Email address of the user

## Testing the Custom Connector

After creating the connector:

1. Go to the **Test** tab
2. Create a new connection
3. Authenticate with Azure AD
4. Test each action to ensure they work correctly

## Next Steps - Copilot Studio Integration

Once the custom connector is working:

1. Go to https://copilotstudio.microsoft.com
2. Create a new copilot or edit existing one
3. Go to Settings > Generative AI
4. Add actions from your Clara custom connector
5. Create topics that use these actions

## Sample Copilot Studio Topics

### Topic: Check License Usage
**Trigger phrases**: "license usage", "how many licenses", "copilot usage"
**Action**: GetLicenseCounts
**Response**: Format the data showing total, used, and available licenses

### Topic: Find Inactive Users
**Trigger phrases**: "inactive users", "unused licenses", "who hasn't used copilot"
**Action**: GetUsageReport
**Response**: List users who haven't used Copilot recently

### Topic: Assign License
**Trigger phrases**: "assign license", "give copilot access", "add user"
**Action**: AssignLicenseByEmail
**Response**: Confirm license assignment

### Topic: Remove License
**Trigger phrases**: "remove license", "revoke access", "take away copilot"
**Action**: RemoveLicenseByEmail
**Response**: Confirm license removal
