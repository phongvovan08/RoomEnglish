using Microsoft.AspNetCore.Http.HttpResults;

namespace RoomEnglish.Web.Endpoints;

public class AppInfo : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.AllowAnonymous();
        groupBuilder.MapGet(GetAppInfo, "info");
    }

    public Ok<AppInfoResponse> GetAppInfo(IConfiguration configuration)
    {
        var version = configuration["App:Version"] ?? "1.0";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        return TypedResults.Ok(new AppInfoResponse
        {
            Version = version,
            Environment = environment
        });
    }
}

public record AppInfoResponse
{
    public string Version { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
}
