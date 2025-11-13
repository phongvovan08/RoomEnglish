using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Constants;
using RoomEnglish.Infrastructure.Data;
using RoomEnglish.Infrastructure.Data.Interceptors;
using RoomEnglish.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("RoomEnglishDb");
        Guard.Against.Null(connectionString, message: "Connection string 'RoomEnglishDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // Read token expiration from configuration (default 7 days)
        var tokenExpirationDays = builder.Configuration.GetValue<int>("Authentication:BearerToken:ExpirationDays", 7);

        // Check if Google OAuth is configured
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        var hasGoogleAuth = !string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret);

        // Configure authentication with appropriate defaults
        // For API calls: use BearerScheme as default to handle Bearer tokens
        // For Google OAuth: ApplicationScheme for sign-in, ExternalScheme for challenge
        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.BearerScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = hasGoogleAuth 
                ? IdentityConstants.ApplicationScheme 
                : IdentityConstants.BearerScheme;
        });

        authBuilder.AddBearerToken(IdentityConstants.BearerScheme, options =>
        {
            // Configure Bearer Token expiration from appsettings.json
            options.BearerTokenExpiration = TimeSpan.FromDays(tokenExpirationDays);
        });

        // Add Identity cookies for Google OAuth sign-in flow
        authBuilder.AddIdentityCookies();

        // Add Google Authentication if credentials are configured
        if (hasGoogleAuth)
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId!;
                options.ClientSecret = googleClientSecret!;
                
                // Google callback path - must be registered in Google Cloud Console
                options.CallbackPath = "/signin-google";

                // Request email and profile information from Google
                options.Scope.Add("email");
                options.Scope.Add("profile");

                // Save tokens for later use (optional)
                options.SaveTokens = true;
                
                // Use External scheme to preserve login info between requests
                options.SignInScheme = IdentityConstants.ExternalScheme;
                
                // Handle authentication callbacks with custom logic
                // See GoogleOAuthHandler for user creation and token generation
                options.Events = GoogleOAuthHandler.CreateGoogleOAuthEvents();
            });
        }

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
