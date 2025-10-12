using Azure.Identity;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Infrastructure.Data;
using RoomEnglish.Web.Services;
using RoomEnglish.Web.Configuration;
using Microsoft.AspNetCore.Mvc;

using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddScoped<IAuthenticationConfiguration, AuthenticationConfiguration>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient();
        
        // Configure authentication settings
        builder.Services.Configure<AuthenticationSettings>(
            builder.Configuration.GetSection(AuthenticationSettings.SectionName));
        
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowVueApp", policy =>
            {
                policy.WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:3001", 
                    "http://localhost:3002",
                    "http://localhost:3003",
                    "https://localhost:3000",
                    "https://localhost:3001",
                    "https://localhost:3002",
                    "https://localhost:3003"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "RoomEnglish API";

            // Add JWT
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
