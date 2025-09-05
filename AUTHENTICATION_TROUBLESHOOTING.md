# 🔐 Power Platform Authentication Troubleshooting Guide

## Current Issue: Token Expired (401 Error)

### ❌ **Error Details:**
```
"The token expired at '09/05/2025 01:00:32'"
```

### ✅ **Quick Fix (Do This Now):**

1. **Open Power Apps Portal**
   - Go to [https://make.powerapps.com](https://make.powerapps.com)
   - Navigate to **Data** → **Custom connectors**

2. **Find Your Clara Connector**
   - Look for "Clara Copilot Agent" or similar name

3. **Refresh Authentication**
   - Click on your connector
   - Go to **"Test" tab**
   - Click **"New connection"** button
   - **Re-authenticate** when prompted
   - Use the same Azure AD credentials

4. **Test Again**
   - Try the "Get License Counts" operation
   - Should work without 401 error

### 🛠️ **Alternative Fix:**

If "New connection" doesn't work:

1. **Delete Existing Connection**
   - Go to **Data** → **Connections**
   - Find your Clara connector connection
   - Delete it

2. **Create Fresh Connection**
   - Go back to **Custom connectors** → **Clara connector** → **Test**
   - Click **"New connection"**
   - Complete OAuth flow again

### 🔍 **Why This Happened:**

- **OAuth tokens expire** for security (1-2 hours standard)
- **Token lifetime policies are deprecated** by Microsoft (September 2025)
- **Cannot extend token lifetime** beyond Azure AD defaults
- Your token expired at **1:00 AM** this morning
- You tested at **5:44 AM** (4+ hours later)
- Power Platform should auto-refresh but sometimes needs manual trigger

### ⚠️ **Important Update:**
Microsoft has **deprecated token lifetime policies**. Maximum token lifetime is now:
- **Access Tokens**: 1-2 hours (cannot be extended)
- **Refresh Tokens**: 24 hours to 90 days (automatic)

This means **you'll need to re-authenticate every 1-2 hours** - this is normal and secure.

### 🚫 **Common Mistakes:**

- ❌ Don't change the API or connector settings
- ❌ Don't re-import the Swagger spec (not needed for auth issues)
- ❌ Don't modify Azure AD app (token expiration is normal)

### ✅ **Expected Result:**

After re-authentication, you should get a successful response like:
```json
{
  "totalLicenses": 100,
  "usedLicenses": 75,
  "availableLicenses": 25
}
```

### 📞 **Still Having Issues?**

If re-authentication doesn't work:
1. Check if your Azure AD user has proper permissions
2. Verify the redirect URI is still correct: `https://global.consent.azure-apim.net/redirect`
3. Try testing in an incognito/private browser window

---
**Last Updated**: September 5, 2025  
**Issue Type**: Authentication (Token Expiration)  
**Status**: Awaiting user re-authentication
