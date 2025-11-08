using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoomEnglish.Infrastructure.Identity;
using System.Security.Claims;

namespace RoomEnglish.Web.Endpoints;

public class GoogleAuth : EndpointGroupBase
{
    public override string? GroupName => "google-auth";

    public override void Map(RouteGroupBuilder group)
    {
        group.WithTags("GoogleAuth");

        group.MapGet("login-google", LoginGoogle)
            .WithName("LoginGoogle")
            .WithSummary("Khởi tạo đăng nhập bằng Google")
            .WithOpenApi()
            .AllowAnonymous();
    }

    /// <summary>
    /// Initiates Google OAuth login flow
    /// </summary>
    public IResult LoginGoogle(HttpContext context, string? returnUrl = null)
    {
        // Store returnUrl in Items to be used in OnTicketReceived event
        var properties = new AuthenticationProperties
        {
            Items = { ["returnUrl"] = returnUrl ?? "http://localhost:3000/auth/login" }
        };

        return Results.Challenge(properties, ["Google"]);
    }

    /// <summary>
    /// Handles Google OAuth callback and creates/links user account
    /// This is called after Google redirects to /signin-google and middleware processes it
    /// </summary>
    public async Task<IResult> GoogleCallback(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<GoogleAuth> logger)
    {
        var returnUrl = context.Request.Query["returnUrl"].ToString() ?? "http://localhost:3000/login";
        
        logger.LogInformation("🔍 GoogleCallback called - ReturnUrl: {ReturnUrl}", returnUrl);
        logger.LogInformation("📝 Cookies: {Cookies}", string.Join(", ", context.Request.Cookies.Keys));
        
        // Try to authenticate with External scheme to get Google user info
        var externalResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        logger.LogInformation("🔐 External auth result: {IsSuccess}", externalResult?.Succeeded);
        
        if (externalResult?.Succeeded != true)
        {
            logger.LogError("❌ External authentication failed");
            return Results.Redirect($"{returnUrl}?error=external_auth_failed");
        }
        
        // Get the information about the user from the external login provider
        var info = await signInManager.GetExternalLoginInfoAsync();
        logger.LogInformation("� External login info: {HasInfo}", info != null);
        
        if (info == null)
        {
            logger.LogError("❌ External login info is null");
            return Results.Redirect($"{returnUrl}?error=external_login_failed");
        }

        // Sign in the user with this external login provider if the user already has a login
        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            // User already exists and is linked to this Google account
            var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                // Generate bearer token
                var token = await GenerateBearerToken(context, user, signInManager);
                return Results.Redirect($"{returnUrl}?token={token}");
            }
        }

        if (result.IsLockedOut)
        {
            return Results.Redirect($"{returnUrl}?error=account_locked");
        }

        // If the user does not have an account, create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return Results.Redirect($"{returnUrl}?error=email_not_provided");
        }

        // Check if user with this email already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Link Google account to existing user
            var addLoginResult = await userManager.AddLoginAsync(existingUser, info);
            if (addLoginResult.Succeeded)
            {
                await signInManager.SignInAsync(existingUser, isPersistent: true);
                var token = await GenerateBearerToken(context, existingUser, signInManager);
                return Results.Redirect($"{returnUrl}?token={token}");
            }
            else
            {
                return Results.Redirect($"{returnUrl}?error=link_account_failed");
            }
        }

        // Create new user
        var userName = email.Split('@')[0];
        var newUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true // Google has verified the email
        };

        var createResult = await userManager.CreateAsync(newUser);
        if (createResult.Succeeded)
        {
            // Link Google account to the new user
            await userManager.AddLoginAsync(newUser, info);
            await signInManager.SignInAsync(newUser, isPersistent: true);

            var token = await GenerateBearerToken(context, newUser, signInManager);
            return Results.Redirect($"{returnUrl}?token={token}");
        }

        return Results.Redirect($"{returnUrl}?error=create_user_failed");
    }

    private async Task<string> GenerateBearerToken(
        HttpContext context,
        ApplicationUser user,
        SignInManager<ApplicationUser> signInManager)
    {
        // Create a ClaimsPrincipal for the user
        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        // Use the bearer token protector to generate a token
        // This is the same way Identity API generates tokens for /login endpoint
        var timeProvider = context.RequestServices.GetRequiredService<TimeProvider>();
        var optionsMonitor = context.RequestServices.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        var options = optionsMonitor.Get(IdentityConstants.BearerScheme);

        var ticket = new AuthenticationTicket(claimsPrincipal, IdentityConstants.BearerScheme);
        var currentUtc = timeProvider.GetUtcNow();
        ticket.Properties.IssuedUtc = currentUtc;
        ticket.Properties.ExpiresUtc = currentUtc.Add(options.BearerTokenExpiration);

        var token = options.BearerTokenProtector.Protect(ticket);
        return token;
    }
}
