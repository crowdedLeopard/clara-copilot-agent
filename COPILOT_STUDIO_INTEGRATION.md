# Clara Copilot Studio Integration Guide

## Step-by-Step Integration Process

### Phase 1: External API Access Setup

1. **Get ngrok URL**:
   - After running `ngrok http https://localhost:56553`
   - Copy the HTTPS forwarding URL (e.g., `https://abc123.ngrok-free.app`)

2. **Update Azure App Registration**:
   ```powershell
   az ad app update --id 1df3409e-9981-4954-bf7c-101cb3dde1ff --public-client-redirect-uris "https://YOUR_NGROK_URL/swagger/oauth2-redirect.html"
   ```

### Phase 2: Create Power Platform Custom Connector

1. **Go to Power Platform admin center**:
   - Visit: https://make.powerapps.com
   - Select your environment
   - Go to "Data" > "Custom connectors"

2. **Create New Custom Connector**:
   - Click "New custom connector" > "Create from blank"
   - Name: "Clara Copilot Agent Connector"

3. **General Settings**:
   - Host: `YOUR_NGROK_URL` (without https://)
   - Base URL: `/`
   - Authentication Type: OAuth 2.0

4. **Security Configuration**:
   - Identity Provider: Azure Active Directory
   - Client ID: `20aed42e-9b90-40b7-94cd-e171bd3e15ac` (Clara API App ID)
   - Client Secret: (Create new secret in Azure AD)
   - Authorization URL: `https://login.microsoftonline.com/6a4093fe-58cd-40de-9d05-63fd36ee663d/oauth2/v2.0/authorize`
   - Token URL: `https://login.microsoftonline.com/6a4093fe-58cd-40de-9d05-63fd36ee663d/oauth2/v2.0/token`
   - Refresh URL: `https://login.microsoftonline.com/6a4093fe-58cd-40de-9d05-63fd36ee663d/oauth2/v2.0/token`
   - Scope: `api://20aed42e-9b90-40b7-94cd-e171bd3e15ac/access_as_user`

### Phase 3: Define API Actions

Add the following actions to your custom connector:

#### Action 1: Get License Usage
- **Operation ID**: `GetLicenseUsage`
- **Summary**: Get M365 Copilot license usage data
- **Request**:
  - Method: GET
  - URL: `/api/license/usage`

#### Action 2: Get Available Licenses
- **Operation ID**: `GetAvailableLicenses`
- **Summary**: Get available M365 Copilot licenses
- **Request**:
  - Method: GET
  - URL: `/api/license/available`

#### Action 3: Get User Reports
- **Operation ID**: `GetUserReports`
- **Summary**: Get user activity reports
- **Request**:
  - Method: GET
  - URL: `/api/reports/users`

### Phase 4: Import to Copilot Studio

1. **Open Copilot Studio**:
   - Visit: https://copilotstudio.microsoft.com
   - Select your environment

2. **Create New Copilot**:
   - Click "Create" > "New copilot"
   - Name: "Clara License Manager"
   - Description: "AI assistant for M365 Copilot license management"

3. **Add Custom Connector**:
   - Go to "Settings" > "Generative AI"
   - Under "Actions", click "Add action"
   - Select "Choose an action"
   - Find your "Clara Copilot Agent Connector"
   - Select the actions you want to enable

4. **Configure Topics**:
   - Create topics for license management scenarios
   - Use the Clara API actions in your conversation flows

### Phase 5: Test the Integration

1. **Test Custom Connector**:
   - In Power Platform, test each action
   - Verify authentication works
   - Check response data format

2. **Test in Copilot Studio**:
   - Use the test chat to interact with Clara
   - Try queries like:
     - "Show me current license usage"
     - "How many Copilot licenses are available?"
     - "Generate a user activity report"

## Sample Copilot Studio Topics

### Topic: Check License Usage
**Trigger phrases**: "license usage", "how many licenses", "copilot usage"

**Actions**:
1. Call GetLicenseUsage action
2. Parse response
3. Display formatted results

### Topic: Available Licenses
**Trigger phrases**: "available licenses", "free licenses", "remaining licenses"

**Actions**:
1. Call GetAvailableLicenses action
2. Calculate available vs. used
3. Provide recommendations

## Security Considerations

1. **Production Deployment**:
   - Deploy Clara API to Azure App Service
   - Use proper SSL certificates
   - Implement proper CORS policies

2. **Authentication**:
   - Use managed identities in production
   - Implement proper role-based access
   - Regular token rotation

3. **Monitoring**:
   - Enable Application Insights
   - Set up alerts for API failures
   - Monitor usage patterns

## Troubleshooting

### Common Issues:

1. **Authentication Failures**:
   - Check redirect URIs match exactly
   - Verify client IDs are correct
   - Ensure admin consent is granted

2. **API Connection Issues**:
   - Verify ngrok tunnel is active
   - Check firewall settings
   - Validate SSL certificates

3. **Copilot Studio Integration**:
   - Ensure custom connector is working
   - Check action permissions
   - Verify response formats match expected schema
