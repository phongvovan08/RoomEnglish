namespace RoomEnglish.Web.Configuration;

public class AuthenticationSettings
{
    public const string SectionName = "Authentication";
    
    public DefaultCredentials DefaultCredentials { get; set; } = new();
}

public class DefaultCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}