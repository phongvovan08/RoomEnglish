namespace RoomEnglish.Application.Common.Interfaces;

public interface IAuthenticationConfiguration
{
    string DefaultEmail { get; }
    string DefaultPassword { get; }
    string BaseUrl { get; }
}