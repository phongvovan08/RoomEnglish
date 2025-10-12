using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Web.Configuration;
using Microsoft.Extensions.Options;

namespace RoomEnglish.Web.Services;

public class AuthenticationConfiguration : IAuthenticationConfiguration
{
    private readonly AuthenticationSettings _settings;

    public AuthenticationConfiguration(IOptions<AuthenticationSettings> settings)
    {
        _settings = settings.Value;
    }

    public string DefaultEmail => _settings.DefaultCredentials.Email;
    public string DefaultPassword => _settings.DefaultCredentials.Password;
    public string BaseUrl => "https://localhost:5001"; // Could also be from configuration
}