using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Queries.GetUserProgress;
using RoomEnglish.Application.Vocabulary.Commands.UpdateUserProgress;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Application.Vocabulary.Commands;
using RoomEnglish.Web.Infrastructure;

namespace RoomEnglish.Web.Endpoints;

public class UserProgress : EndpointGroupBase
{
    public override string? GroupName => "user-progress";

    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization()
             .WithTags("User Progress")
             .WithOpenApi();

        group.MapGet("", GetUserProgress)
             .WithName("GetUserProgress")
             .WithSummary("Get user progress")
             .WithDescription("Retrieves user progress for categories, words, and examples");

        group.MapGet("category/{categoryId}", GetCategoryProgress)
             .WithName("GetCategoryProgress")
             .WithSummary("Get category progress")
             .WithDescription("Retrieves user progress for a specific category");

        group.MapPost("word/{wordId}", UpdateWordProgress)
             .WithName("UpdateWordProgress")
             .WithSummary("Update word progress")
             .WithDescription("Updates user progress after practicing a word");

        group.MapPost("example/{exampleId}", UpdateExampleProgress)
             .WithName("UpdateExampleProgress")
             .WithSummary("Update example progress")
             .WithDescription("Updates user progress after completing an example");

        group.MapPost("category/{categoryId}/recalculate", RecalculateCategoryProgress)
             .WithName("RecalculateCategoryProgress")
             .WithSummary("Recalculate category progress")
             .WithDescription("Recalculates and updates category progress based on word progress");

        group.MapGet("position/{wordId}", GetLearningPosition)
             .WithName("GetLearningPosition")
             .WithSummary("Get learning position")
             .WithDescription("Gets user's saved learning position for a word");

        group.MapPost("position", SaveLearningPosition)
             .WithName("SaveLearningPosition")
             .WithSummary("Save learning position")
             .WithDescription("Saves user's current learning position");

        group.MapDelete("position/{wordId}", ClearLearningPosition)
             .WithName("ClearLearningPosition")
             .WithSummary("Clear learning position")
             .WithDescription("Clears user's saved learning position for a word");
    }

    [Authorize]
    public async Task<IResult> GetUserProgress(
        ISender sender,
        [AsParameters] GetUserProgressQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> GetCategoryProgress(
        ISender sender,
        int categoryId)
    {
        var result = await sender.Send(new GetUserProgressQuery { CategoryId = categoryId });
        return Results.Ok(result);
    }

    public record UpdateWordProgressRequest(bool IsCorrect);

    [Authorize]
    public async Task<IResult> UpdateWordProgress(
        ISender sender,
        int wordId,
        UpdateWordProgressRequest request)
    {
        await sender.Send(new UpdateWordProgressCommand 
        { 
            WordId = wordId,
            IsCorrect = request.IsCorrect
        });
        return Results.Ok();
    }

    public record UpdateExampleProgressRequest(int AccuracyPercentage);

    [Authorize]
    public async Task<IResult> UpdateExampleProgress(
        ISender sender,
        int exampleId,
        UpdateExampleProgressRequest request)
    {
        await sender.Send(new UpdateExampleProgressCommand 
        { 
            ExampleId = exampleId,
            AccuracyPercentage = request.AccuracyPercentage
        });
        return Results.Ok();
    }

    [Authorize]
    public async Task<IResult> RecalculateCategoryProgress(
        ISender sender,
        int categoryId)
    {
        await sender.Send(new UpdateCategoryProgressCommand { CategoryId = categoryId });
        return Results.Ok();
    }

    [Authorize]
    public async Task<IResult> GetLearningPosition(
        ISender sender,
        int wordId)
    {
        var result = await sender.Send(new GetLearningPositionQuery { WordId = wordId });
        return Results.Ok(result);
    }

    public record SaveLearningPositionRequest(int WordId, int GroupIndex, int LastExampleIndex);

    [Authorize]
    public async Task<IResult> SaveLearningPosition(
        ISender sender,
        SaveLearningPositionRequest request)
    {
        await sender.Send(new SaveLearningPositionCommand 
        { 
            WordId = request.WordId,
            GroupIndex = request.GroupIndex,
            LastExampleIndex = request.LastExampleIndex
        });
        return Results.Ok();
    }

    [Authorize]
    public async Task<IResult> ClearLearningPosition(
        ISender sender,
        int wordId)
    {
        await sender.Send(new ClearLearningPositionCommand { WordId = wordId });
        return Results.Ok();
    }
}
