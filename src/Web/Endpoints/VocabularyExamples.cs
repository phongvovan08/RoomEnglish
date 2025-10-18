using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyExamples;

namespace RoomEnglish.Web.Endpoints;

public class VocabularyExamples : EndpointGroupBase
{
    public override string? GroupName => "vocabulary-examples";
    
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization()
             .WithTags("Vocabulary Examples")
             .WithOpenApi();

        group.MapGet("", GetExamples)
             .WithName("GetVocabularyExamples")
             .WithSummary("Get vocabulary examples")
             .WithDescription("Retrieves vocabulary examples, optionally filtered by vocabulary word ID");
        group.MapPost("", CreateExample)
             .WithName("CreateVocabularyExample")
             .WithSummary("Create new example")
             .WithDescription("Creates a new vocabulary example");
        group.MapPut("{id}", UpdateExample)
             .WithName("UpdateVocabularyExample")
             .WithSummary("Update example")
             .WithDescription("Updates an existing vocabulary example");
        group.MapDelete("{id}", DeleteExample)
             .WithName("DeleteVocabularyExample")
             .WithSummary("Delete example")
             .WithDescription("Soft deletes a vocabulary example");
    }

    public record UpsertExampleRequest(
        string Sentence,
        string Translation,
        string? Grammar,
        int VocabularyWordId);

    [Authorize]
    public async Task<IResult> GetExamples(
        ISender sender,
        [AsParameters] GetVocabularyExamplesQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateExample(
        IApplicationDbContext context,
        UpsertExampleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sentence) || string.IsNullOrWhiteSpace(request.Translation))
        {
            return Results.BadRequest("Sentence and Translation are required");
        }

        var wordExists = await context.VocabularyWords.AnyAsync(w => w.Id == request.VocabularyWordId && w.IsActive);
        if (!wordExists) return Results.BadRequest("Invalid vocabularyWordId");

        var entity = new RoomEnglish.Domain.Entities.VocabularyExample
        {
            Sentence = request.Sentence.Trim(),
            Translation = request.Translation.Trim(),
            Grammar = request.Grammar?.Trim() ?? string.Empty,
            WordId = request.VocabularyWordId,
            IsActive = true
        };

        context.VocabularyExamples.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { id = entity.Id });
    }

    [Authorize]
    public async Task<IResult> UpdateExample(
        IApplicationDbContext context,
        int id,
        UpsertExampleRequest request)
    {
        var entity = await context.VocabularyExamples.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Results.NotFound();

        if (!string.IsNullOrWhiteSpace(request.Sentence)) entity.Sentence = request.Sentence.Trim();
        if (!string.IsNullOrWhiteSpace(request.Translation)) entity.Translation = request.Translation.Trim();
        entity.Grammar = request.Grammar?.Trim() ?? entity.Grammar;
        if (request.VocabularyWordId != 0) entity.WordId = request.VocabularyWordId;

        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }

    [Authorize]
    public async Task<IResult> DeleteExample(
        IApplicationDbContext context,
        int id)
    {
        var entity = await context.VocabularyExamples.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Results.NotFound();

        entity.IsActive = false;
        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }
}
