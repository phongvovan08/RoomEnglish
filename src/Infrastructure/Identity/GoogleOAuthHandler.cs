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
            OnRedirectToAuthorizationEndpoint = context =>
            {
                // Add prompt=select_account to force account selection
                context.RedirectUri += "&prompt=select_account";
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
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
            
            // Update DisplayName if it's empty and we have a name from Google
            if (string.IsNullOrEmpty(user.DisplayName) && !string.IsNullOrEmpty(name))
            {
                user.DisplayName = name;
                await userManager.UpdateAsync(user);
                logger.LogInformation("Google OAuth: Updated DisplayName to {DisplayName} for existing user {Email}", name, email);
            }
            
            return user;
        }

        // Create new user
        logger.LogInformation("Google OAuth: Creating new user {Email} with name {Name}", email, name);
        
        // UserName: unique identifier (lowercase email prefix or custom)
        var userName = email.Split('@')[0].ToLower(); // e.g., "phongvv198"
        
        // Display name from Google (e.g., "Phòng Võ")
        var displayName = !string.IsNullOrEmpty(name) ? name : userName;
        
        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName, // Store full name with special characters
            EmailConfirmed = true // Google has verified the email
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Google OAuth: Failed to create user {Email}. Errors: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        // Assign default "User" role
        var roleResult = await userManager.AddToRoleAsync(user, Domain.Constants.Roles.User);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Google OAuth: Failed to assign User role to {Email}. Errors: {Errors}",
                email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
        else
        {
            logger.LogInformation("Google OAuth: Assigned User role to {Email}", email);
        }

        logger.LogInformation("Google OAuth: Successfully created user {Email} with UserName {UserName} and DisplayName {DisplayName}", 
            email, userName, displayName);
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
