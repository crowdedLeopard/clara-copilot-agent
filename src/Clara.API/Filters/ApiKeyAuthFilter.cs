using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Clara.API.Filters
{
    public class ApiKeyAuthFilter : Attribute, IAuthorizationFilter
    {
        private const string API_KEY_HEADER = "X-API-Key";
        private const string API_KEY_VALUE = "clara-api-key-2025"; // Simple API key for testing

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip API key check if user is already authenticated via OAuth
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                return;
            }

            // Check for API key in header
            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "API Key missing. Provide X-API-Key header or use OAuth authentication." });
                return;
            }

            if (!API_KEY_VALUE.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid API Key" });
                return;
            }
        }
    }
}
