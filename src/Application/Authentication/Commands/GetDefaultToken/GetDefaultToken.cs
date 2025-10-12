using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using System.Text.Json;

namespace RoomEnglish.Application.Authentication.Commands.GetDefaultToken;

public record GetDefaultTokenCommand : IRequest<string>;

public class GetDefaultTokenCommandHandler : IRequestHandler<GetDefaultTokenCommand, string>
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationConfiguration _authConfig;

    public GetDefaultTokenCommandHandler(HttpClient httpClient, IAuthenticationConfiguration authConfig)
    {
        _httpClient = httpClient;
        _authConfig = authConfig;
    }

    public async Task<string> Handle(GetDefaultTokenCommand request, CancellationToken cancellationToken)
    {
        // Create login request với tài khoản mặc định từ configuration
        var loginRequest = new
        {
            email = _authConfig.DefaultEmail,
            password = _authConfig.DefaultPassword,
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

            var token = tokenResponse.GetProperty("accessToken").GetString() ?? string.Empty;
            return $"Bearer {token}";
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Đăng nhập thất bại với tài khoản mặc định: {errorContent}");
        }
    }
}
