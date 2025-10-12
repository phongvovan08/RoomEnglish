using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using System.Text.Json;

namespace RoomEnglish.Application.Authentication.Commands.GetToken;

public record GetTokenCommand : IRequest<TokenResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class GetTokenCommandHandler : IRequestHandler<GetTokenCommand, TokenResponse>
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationConfiguration _authConfig;

    public GetTokenCommandHandler(HttpClient httpClient, IAuthenticationConfiguration authConfig)
    {
        _httpClient = httpClient;
        _authConfig = authConfig;
    }

    public async Task<TokenResponse> Handle(GetTokenCommand request, CancellationToken cancellationToken)
    {
        // Create login request
        var loginRequest = new
        {
            email = request.Email,
            password = request.Password,
            twoFactorCode = (string?)null,
            twoFactorRecoveryCode = (string?)null
        };
        
        // Get base URL from configuration
        var baseUrl = _authConfig.BaseUrl;
        
        // Call login endpoint
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(loginJson, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{baseUrl}/api/Users/login", content, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return new TokenResponse
            {
                AccessToken = tokenResponse.GetProperty("accessToken").GetString() ?? "",
                RefreshToken = tokenResponse.GetProperty("refreshToken").GetString() ?? "",
                TokenType = tokenResponse.GetProperty("tokenType").GetString() ?? "Bearer",
                ExpiresIn = tokenResponse.GetProperty("expiresIn").GetInt64()
            };
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Đăng nhập thất bại: {errorContent}");
        }
    }
}