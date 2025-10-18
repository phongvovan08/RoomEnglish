using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWords;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWordDetail;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
        group.MapPost("upload-excel", UploadExcel)
             .WithName("UploadVocabularyWordsExcel")
             .WithSummary("Upload Excel file")
             .WithDescription("Imports vocabulary words from an Excel file");
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

    public record UploadExcelResult(
        bool Success,
        int AddedCount,
        int UpdatedCount,
        List<string> Errors);

    [Authorize]
    public async Task<IResult> UploadExcel(
        HttpRequest httpRequest,
        IApplicationDbContext context,
        int categoryId)
    {
        if (!httpRequest.HasFormContentType)
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        var form = await httpRequest.ReadFormAsync();
        var file = form.Files.GetFile("file");

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
        {
            return Results.BadRequest("Only Excel files (.xlsx, .xls) are allowed");
        }

        // Verify category exists
        var categoryExists = await context.VocabularyCategories.AnyAsync(c => c.Id == categoryId && c.IsActive);
        if (!categoryExists)
        {
            return Results.BadRequest("Invalid categoryId");
        }

        var result = new UploadExcelResult(false, 0, 0, new List<string>());

        try
        {
            using var package = new OfficeOpenXml.ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                result = result with { Errors = ["No worksheet found in Excel file"] };
                return Results.Ok(result);
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1)
            {
                result = result with { Errors = ["Excel file contains no data rows"] };
                return Results.Ok(result);
            }

            // Process data rows (assuming headers: Word, Definition, Pronunciation)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var word = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var definition = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var pronunciation = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(definition))
                    {
                        result.Errors.Add($"Row {row}: Word and Definition are required");
                        continue;
                    }

                    // Check if word already exists
                    var existingWord = await context.VocabularyWords
                        .FirstOrDefaultAsync(w => w.Word.ToLower() == word.ToLower() && w.CategoryId == categoryId);

                    if (existingWord != null)
                    {
                        // Update existing word
                        existingWord.Definition = definition;
                        existingWord.Phonetic = pronunciation ?? existingWord.Phonetic;
                        result = result with { UpdatedCount = result.UpdatedCount + 1 };
                    }
                    else
                    {
                        // Create new word
                        var newWord = new RoomEnglish.Domain.Entities.VocabularyWord
                        {
                            Word = word,
                            Definition = definition,
                            Phonetic = pronunciation ?? string.Empty,
                            CategoryId = categoryId,
                            IsActive = true
                        };
                        context.VocabularyWords.Add(newWord);
                        result = result with { AddedCount = result.AddedCount + 1 };
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }
            }

            await context.SaveChangesAsync(CancellationToken.None);
            result = result with { Success = true };
        }
        catch (Exception ex)
        {
            result = result with { Errors = [$"Error processing Excel file: {ex.Message}"] };
        }

        return Results.Ok(result);
    }
}
