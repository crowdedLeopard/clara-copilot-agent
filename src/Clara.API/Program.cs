using Azure.Identity;
using Clara.API.Classes;
using Clara.API.Interfaces;
using Clara.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add configuration for Azure AD to be used by Graph
var config = builder.Configuration;
builder.Services.AddSingleton(new ClientSecretCredential(
    config["AzureAd:TenantId"],
    config["AzureAd:ClientId"],
    config["AzureAd:ClientSecret"]
));


builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var credential = sp.GetRequiredService<ClientSecretCredential>();
    return new GraphServiceClient(credential);
});

builder.Services.AddScoped<ICopilotUsageService, CopilotUsageService>();
builder.Services.AddScoped<ICopilotLicenseService, CopilotLicenseService>();
builder.Services.AddScoped<ICopilotGroupService, CopilotGroupService>();



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Clara API", Version = "v1" });

    // Add OAuth2 Security Definition
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{config["AzureAd:Instance"]}{config["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{config["AzureAd:Instance"]}{config["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{config["AzureAd:ClientId"]}/access_as_user", "Access API as user" }
                }
            }
        }
    });

    // Add Security Requirement - This was missing!
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "oauth2" 
                }
            },
            new[] { $"api://{config["AzureAd:ClientId"]}/access_as_user" }
        }
    });

    // Optional: Use operation filter if you want more control over which endpoints require auth
    // c.OperationFilter<AuthorizeCheckOperationFilter>();
});



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization options
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();


// Apply CORS middleware
app.UseCors("AllowSwaggerUI");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
   app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clara API V1");
        c.OAuthClientId(config["Swagger:ClientId"]); // SPA client ID
        c.OAuthUsePkce();                            // Required for PKCE
        c.OAuthScopes($"api://{config["AzureAd:ClientId"]}/access_as_user");
//        c.OAuth2RedirectUrl("https://your-ngrok-subdomain.ngrok.io/swagger/oauth2-redirect.html"); // Update to ngrok URL
    });

}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();