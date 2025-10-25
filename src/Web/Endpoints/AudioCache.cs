using MediatR;
using Microsoft.AspNetCore.Authorization;
using RoomEnglish.Application.Audio.Queries;
using RoomEnglish.Application.Audio.Commands;
using RoomEnglish.Web.Infrastructure;

namespace RoomEnglish.Web.Endpoints;

public class AudioCache : EndpointGroupBase
{
    public override string? GroupName => "audio-cache";

    public override void Map(RouteGroupBuilder group)
    {
        group.WithTags("Audio Cache")
             .WithOpenApi();

        group.MapGet("", GetAudioCache)
             .WithName("GetAudioCache")
             .WithSummary("Get cached audio")
             .WithDescription("Retrieves cached audio from database");

        group.MapPost("", SaveAudioCache)
             .WithName("SaveAudioCache")
             .WithSummary("Save audio to cache")
             .WithDescription("Saves audio data to database cache");

        group.MapGet("stats", GetCacheStats)
             .WithName("GetAudioCacheStats")
             .WithSummary("Get cache statistics")
             .WithDescription("Gets statistics about audio cache");

        group.MapPost("cleanup", CleanupCache)
             .WithName("CleanupAudioCache")
             .WithSummary("Cleanup cache")
             .WithDescription("Removes expired and least used cache entries");
    }

    public async Task<IResult> GetAudioCache(
        ISender sender,
        [AsParameters] GetAudioCacheQuery query)
    {
        var result = await sender.Send(query);
        
        if (result == null)
        {
            return Results.NotFound();
        }

        return Results.File(result.AudioData, result.MimeType);
    }

    public record SaveAudioRequest(
        string Text,
        string Voice,
        double Rate,
        string Provider,
        string AudioDataBase64,
        string MimeType,
        int? ExpiryDays);

    public async Task<IResult> SaveAudioCache(
        ISender sender,
        SaveAudioRequest request)
    {
        var audioData = Convert.FromBase64String(request.AudioDataBase64);

        await sender.Send(new SaveAudioCacheCommand
        {
            Text = request.Text,
            Voice = request.Voice,
            Rate = request.Rate,
            Provider = request.Provider,
            AudioData = audioData,
            MimeType = request.MimeType,
            ExpiryDays = request.ExpiryDays
        });

        return Results.Ok();
    }

    public async Task<IResult> GetCacheStats(ISender sender)
    {
        var stats = await sender.Send(new GetAudioCacheStatsQuery());
        return Results.Ok(stats);
    }

    public record CleanupRequest(int? MaxCacheSizeMB, bool DeleteExpired);

    [Authorize]
    public async Task<IResult> CleanupCache(
        ISender sender,
        CleanupRequest request)
    {
        var deletedCount = await sender.Send(new CleanupAudioCacheCommand
        {
            MaxCacheSizeMB = request.MaxCacheSizeMB,
            DeleteExpired = request.DeleteExpired
        });

        return Results.Ok(new { DeletedEntries = deletedCount });
    }
}
