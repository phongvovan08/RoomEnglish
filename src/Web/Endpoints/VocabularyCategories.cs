using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyCategories;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RoomEnglish.Web.Endpoints;

public class VocabularyCategories : EndpointGroupBase
{
    public override string? GroupName => "vocabulary-categories";
    
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization()
             .WithTags("Vocabulary Categories")
             .WithOpenApi();

        group.MapGet("", GetCategories)
             .WithName("GetVocabularyCategories")
             .WithSummary("Get all vocabulary categories")
             .WithDescription("Retrieves a paginated list of vocabulary categories");
        group.MapGet("{id}", GetCategoryById)
             .WithName("GetVocabularyCategoryById")
             .WithSummary("Get category by ID")
             .WithDescription("Retrieves a specific vocabulary category by its ID");
        group.MapPost("", CreateCategory)
             .WithName("CreateVocabularyCategory")
             .WithSummary("Create new category")
             .WithDescription("Creates a new vocabulary category");
        group.MapPut("{id}", UpdateCategory)
             .WithName("UpdateVocabularyCategory")
             .WithSummary("Update category")
             .WithDescription("Updates an existing vocabulary category");
        group.MapDelete("{id}", DeleteCategory)
             .WithName("DeleteVocabularyCategory")
             .WithSummary("Delete category")
             .WithDescription("Soft deletes a vocabulary category");
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
    public async Task<IResult> GetCategoryById(
        IApplicationDbContext context,
        int id)
    {
        var category = await context.VocabularyCategories
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                createdAt = c.Created,
                vocabularyCount = c.Words.Count(w => w.IsActive)
            })
            .FirstOrDefaultAsync(c => c.id == id);

        return category is null ? Results.NotFound() : Results.Ok(category);
    }

    public record UpsertCategoryRequest(string Name, string? Description);

    [Authorize]
    public async Task<IResult> CreateCategory(
        IApplicationDbContext context,
        UpsertCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Name is required");
        }

        var entity = new RoomEnglish.Domain.Entities.VocabularyCategory
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = true
        };

        context.VocabularyCategories.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);

        return Results.Ok(new { id = entity.Id, name = entity.Name, description = entity.Description, createdAt = entity.Created });
    }

    [Authorize]
    public async Task<IResult> UpdateCategory(
        IApplicationDbContext context,
        int id,
        UpsertCategoryRequest request)
    {
        var entity = await context.VocabularyCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return Results.NotFound();

        entity.Name = request.Name?.Trim() ?? entity.Name;
        entity.Description = request.Description?.Trim() ?? entity.Description;

        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }

    [Authorize]
    public async Task<IResult> DeleteCategory(
        IApplicationDbContext context,
        int id)
    {
        var entity = await context.VocabularyCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return Results.NotFound();

        // Soft delete
        entity.IsActive = false;
        await context.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }
}
