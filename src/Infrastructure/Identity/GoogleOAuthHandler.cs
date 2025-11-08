using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoomEnglish.Infrastructure.Identity;
using System.Security.Claims;

namespace RoomEnglish.Infrastructure.Identity;

/// <summary>
/// Handles Google OAuth authentication callbacks
/// </summary>
public static class GoogleOAuthHandler
{
    /// <summary>
    /// Creates OAuth events for handling Google authentication
    /// </summary>
    public static OAuthEvents CreateGoogleOAuthEvents()
    {
        return new OAuthEvents
        {
            OnTicketReceived = HandleTicketReceived
        };
    }

    /// <summary>
    /// Handles the OAuth ticket after successful Google authentication
    /// Creates or links user account and generates Bearer token
    /// </summary>
    private static async Task HandleTicketReceived(TicketReceivedContext context)
    {
        // Get services from DI
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApplicationUser>>();

        // Get return URL from authentication properties
        string? returnUrl = null;
        context.Properties?.Items.TryGetValue("returnUrl", out returnUrl);
        returnUrl ??= "http://localhost:3000/auth/login";

        try
        {
            // Extract email and name from Google claims
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                logger.LogWarning("Google OAuth: Email not provided");
                context.Response.Redirect($"{returnUrl}?error=email_not_provided");
                context.HandleResponse();
                return;
            }

            var name = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Google OAuth: Processing login for {Email}, Name: {Name}", email, name);

            // Get or create user
            var user = await GetOrCreateUserAsync(userManager, email, name, logger);
            if (user == null)
            {
                context.Response.Redirect($"{returnUrl}?error=create_user_failed");
                context.HandleResponse();
                return;
            }

            // Generate Bearer token
            var token = await GenerateBearerTokenAsync(context.HttpContext, user, signInManager);

            logger.LogInformation("Google OAuth: Successfully authenticated {Email}", email);

            // Redirect with token
            context.Response.Redirect($"{returnUrl}?token={token}");
            context.HandleResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google OAuth: Error processing authentication");
            context.Response.Redirect($"{returnUrl}?error=processing_failed");
            context.HandleResponse();
        }
    }

    /// <summary>
    /// Gets existing user or creates a new one
    /// </summary>
    private static async Task<ApplicationUser?> GetOrCreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string? name,
        ILogger logger)
    {
        // Try to find existing user
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            logger.LogInformation("Google OAuth: Found existing user {Email}", email);
            return user;
        }

        // Create new user
        logger.LogInformation("Google OAuth: Creating new user {Email} with name {Name}", email, name);
        
        // Use Google name if available, otherwise use email prefix
        var userName = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0];
        
        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true // Google has verified the email
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Google OAuth: Failed to create user {Email}. Errors: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        logger.LogInformation("Google OAuth: Successfully created user {Email} with UserName {UserName}", email, userName);
        return user;
    }

    /// <summary>
    /// Generates a Bearer token for the authenticated user
    /// </summary>
    private static async Task<string> GenerateBearerTokenAsync(
        HttpContext httpContext,
        ApplicationUser user,
        SignInManager<ApplicationUser> signInManager)
    {
        // Create claims principal for the user
        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        // Get Bearer token options
        var timeProvider = httpContext.RequestServices.GetRequiredService<TimeProvider>();
        var optionsMonitor = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        var bearerOptions = optionsMonitor.Get(IdentityConstants.BearerScheme);

        // Create authentication ticket
        var ticket = new AuthenticationTicket(claimsPrincipal, IdentityConstants.BearerScheme);
        var currentUtc = timeProvider.GetUtcNow();
        ticket.Properties.IssuedUtc = currentUtc;
        ticket.Properties.ExpiresUtc = currentUtc.Add(bearerOptions.BearerTokenExpiration);

        // Protect and return token
        var token = bearerOptions.BearerTokenProtector.Protect(ticket);
        return token;
    }
}
