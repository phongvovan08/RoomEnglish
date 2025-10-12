using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.HttpResults;
using RoomEnglish.Application.Authentication.Commands.GetToken;
using RoomEnglish.Application.Authentication.Commands.GetDefaultToken;
using RoomEnglish.Application.Common.Models;

namespace RoomEnglish.Web.Endpoints;

public sealed class Authentication : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.WithTags("Authentication");

        group.MapPost("GetToken", GetToken)
            .WithName("GetToken")
            .WithSummary("Lấy token xác thực")
            .WithDescription("Tự động đăng nhập và trả về access token để sử dụng cho các API calls")
            .Produces<TokenResponse>(200)
            .ProducesProblem(400)
            .WithOpenApi();

        group.MapPost("GetDefaultToken", GetDefaultToken)
            .WithName("GetDefaultToken")
            .WithSummary("Lấy token với tài khoản mặc định")
            .WithDescription("Tự động đăng nhập bằng tài khoản administrator@localhost và trả về Bearer token")
            .Produces<string>(200, "text/plain")
            .ProducesProblem(400)
            .WithOpenApi();
    }

    public async Task<Ok<TokenResponse>> GetToken(ISender sender, GetTokenCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetDefaultToken(ISender sender)
    {
        var command = new GetDefaultTokenCommand();
        var result = await sender.Send(command);
        return Results.Content(result, "text/plain", System.Text.Encoding.UTF8);
    }
}
