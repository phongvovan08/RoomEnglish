using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Commands.CompleteLearningSession;
using RoomEnglish.Application.Vocabulary.Commands.StartLearningSession;
using RoomEnglish.Application.Vocabulary.Commands.SubmitDictation;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyCategories;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWordDetail;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWords;
using RoomEnglish.Web.Infrastructure;

namespace RoomEnglish.Web.Endpoints;

public class Vocabulary : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();

        // Categories
        group.MapGet("categories", GetCategories);

        // Words  
        group.MapGet("words", GetWords);
        group.MapGet("words/{id}", GetWordDetail);

        // Learning Sessions
        group.MapPost("sessions/start", StartLearningSession);
        group.MapPut("sessions/{id}/complete", CompleteLearningSession);

        // Dictation
        group.MapPost("dictation/submit", SubmitDictation);
    }

    [Authorize]
    public async Task<IResult> GetCategories(
        ISender sender,
        [AsParameters] GetVocabularyCategoriesQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> GetWords(
        ISender sender,
        [AsParameters] GetVocabularyWordsQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> GetWordDetail(
        ISender sender,
        int id,
        [FromHeader(Name = "User-Id")] string? userId = null)
    {
        var result = await sender.Send(new GetVocabularyWordDetailQuery(id, userId));
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> StartLearningSession(
        ISender sender,
        StartLearningSessionCommand command)
    {
        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CompleteLearningSession(
        ISender sender,
        int id,
        CompleteLearningSessionRequest request)
    {
        var command = new CompleteLearningSessionCommand
        {
            SessionId = id,
            CorrectAnswers = request.CorrectAnswers,
            Score = request.Score
        };

        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> SubmitDictation(
        ISender sender,
        SubmitDictationCommand command)
    {
        var result = await sender.Send(command);
        return Results.Ok(result);
    }
}

// Request models
public class CompleteLearningSessionRequest
{
    public int CorrectAnswers { get; set; }
    public int Score { get; set; }
}
