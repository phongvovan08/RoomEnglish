using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWords;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWordDetail;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RoomEnglish.Web.Endpoints;

public class VocabularyWords : EndpointGroupBase
{
    public override string? GroupName => "vocabulary-words";
    
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization()
             .WithTags("Vocabulary Words")
             .WithOpenApi();

        group.MapGet("", GetWords)
             .WithName("GetVocabularyWords")
             .WithSummary("Get vocabulary words")
             .WithDescription("Retrieves a paginated list of vocabulary words with optional filtering");
        group.MapGet("{id}", GetWordDetail)
             .WithName("GetVocabularyWordDetail")
             .WithSummary("Get word details")
             .WithDescription("Retrieves detailed information about a specific vocabulary word");
        group.MapPost("", CreateWord)
             .WithName("CreateVocabularyWord")
             .WithSummary("Create new word")
             .WithDescription("Creates a new vocabulary word");
        group.MapPut("{id}", UpdateWord)
             .WithName("UpdateVocabularyWord")
             .WithSummary("Update word")
             .WithDescription("Updates an existing vocabulary word");
        group.MapDelete("{id}", DeleteWord)
             .WithName("DeleteVocabularyWord")
             .WithSummary("Delete word")
             .WithDescription("Soft deletes a vocabulary word");
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

    public record UpsertWordRequest(
        string Word,
        string Definition,
        string? Pronunciation,
        int CategoryId);

    [Authorize]
    public async Task<IResult> CreateWord(
        IApplicationDbContext context,
        UpsertWordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Word) || string.IsNullOrWhiteSpace(request.Definition))
        {
            return Results.BadRequest("Word and Definition are required");
        }

        var categoryExists = await context.VocabularyCategories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive);
        if (!categoryExists) return Results.BadRequest("Invalid categoryId");

        var entity = new RoomEnglish.Domain.Entities.VocabularyWord
        {
            Word = request.Word.Trim(),
            Definition = request.Definition.Trim(),
            Phonetic = request.Pronunciation?.Trim() ?? string.Empty,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        context.VocabularyWords.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);

        return Results.Ok(new { id = entity.Id });
    }

    [Authorize]
    public async Task<IResult> UpdateWord(
        IApplicationDbContext context,
        int id,
        UpsertWordRequest request)
    {
        var entity = await context.VocabularyWords.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return Results.NotFound();

        if (!string.IsNullOrWhiteSpace(request.Word)) entity.Word = request.Word.Trim();
        if (!string.IsNullOrWhiteSpace(request.Definition)) entity.Definition = request.Definition.Trim();
        entity.Phonetic = request.Pronunciation?.Trim() ?? entity.Phonetic;
        if (request.CategoryId != 0) entity.CategoryId = request.CategoryId;

        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }

    [Authorize]
    public async Task<IResult> DeleteWord(
        IApplicationDbContext context,
        int id)
    {
        var entity = await context.VocabularyWords.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return Results.NotFound();

        entity.IsActive = false;
        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }
}
