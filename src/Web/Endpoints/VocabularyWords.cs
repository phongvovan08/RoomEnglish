using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWords;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWordDetail;
using RoomEnglish.Application.Vocabulary.Commands.ImportVocabularyFromJson;
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
        group.MapPost("import-json", ImportJson)
             .WithName("ImportVocabularyWordsJson")
             .WithSummary("Import from JSON")
             .WithDescription("Imports vocabulary words from JSON data");
        group.MapGet("template.xlsx", GetExcelTemplate)
             .WithName("GetVocabularyWordsTemplate")
             .WithSummary("Download Excel template")
             .WithDescription("Downloads an Excel template for vocabulary words import");
        group.MapGet("template.json", GetJsonTemplate)
             .WithName("GetVocabularyWordsJsonTemplate")
             .WithSummary("Download JSON template")
             .WithDescription("Downloads a JSON template for vocabulary words import");
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

            // Process data rows (columns: Word, Phonetic, PartOfSpeech, Meaning, Definition, DifficultyLevel)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var word = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var phonetic = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var partOfSpeech = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var meaning = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var definition = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                    var difficultyLevelStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(meaning))
                    {
                        result.Errors.Add($"Row {row}: Word and Meaning are required");
                        continue;
                    }

                    // Parse difficulty level
                    if (!int.TryParse(difficultyLevelStr, out var difficultyLevel))
                    {
                        difficultyLevel = 1; // Default difficulty
                    }

                    // Validate difficulty level range
                    if (difficultyLevel < 1 || difficultyLevel > 5)
                    {
                        difficultyLevel = 1;
                    }

                    // Check if word already exists
                    var existingWord = await context.VocabularyWords
                        .FirstOrDefaultAsync(w => w.Word.ToLower() == word.ToLower() && w.CategoryId == categoryId);

                    if (existingWord != null)
                    {
                        // Update existing word
                        existingWord.Phonetic = phonetic ?? existingWord.Phonetic;
                        existingWord.PartOfSpeech = partOfSpeech ?? existingWord.PartOfSpeech;
                        existingWord.Meaning = meaning;
                        existingWord.Definition = definition ?? existingWord.Definition;
                        existingWord.DifficultyLevel = difficultyLevel;
                        result = result with { UpdatedCount = result.UpdatedCount + 1 };
                    }
                    else
                    {
                        // Create new word
                        var newWord = new RoomEnglish.Domain.Entities.VocabularyWord
                        {
                            Word = word,
                            Phonetic = phonetic ?? string.Empty,
                            PartOfSpeech = partOfSpeech ?? string.Empty,
                            Meaning = meaning,
                            Definition = definition ?? string.Empty,
                            DifficultyLevel = difficultyLevel,
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
            result = result with { Errors = [$"Error processing Excel file: {ex.Message} {ex.InnerException?.Message}"] };
        }

        return Results.Ok(result);
    }

    public async Task<IResult> GetExcelTemplate()
    {
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Vocabulary Template");

        // Headers
        worksheet.Cells[1, 1].Value = "Word (English)";
        worksheet.Cells[1, 2].Value = "Phonetic";
        worksheet.Cells[1, 3].Value = "Part of Speech";
        worksheet.Cells[1, 4].Value = "Meaning (Vietnamese)";
        worksheet.Cells[1, 5].Value = "Definition (English)";
        worksheet.Cells[1, 6].Value = "Difficulty Level";

        // Sample data
        var sampleData = new object[,]
        {
            { "hello", "/həˈloʊ/", "interjection", "xin chào", "used as a greeting", 1 },
            { "beautiful", "/ˈbjuːtɪfəl/", "adjective", "đẹp", "pleasing the senses or mind aesthetically", 2 },
            { "computer", "/kəmˈpjuːtər/", "noun", "máy tính", "an electronic device for storing and processing data", 2 },
            { "study", "/ˈstʌdi/", "verb", "học, nghiên cứu", "to learn about something or acquire knowledge", 1 }
        };

        for (int i = 0; i < sampleData.GetLength(0); i++)
        {
            for (int j = 0; j < sampleData.GetLength(1); j++)
            {
                worksheet.Cells[i + 2, j + 1].Value = sampleData[i, j];
            }
        }

        // Format headers
        using (var range = worksheet.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream();
        await package.SaveAsAsync(stream);
        stream.Position = 0;

        return Results.File(stream.ToArray(), 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "vocabulary-words-template.xlsx");
    }

    public record ImportJsonRequest(string JsonData);

    [Authorize]
    public async Task<IResult> ImportJson(
        ISender sender,
        ImportJsonRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.JsonData))
            {
                return Results.BadRequest("JSON data is required");
            }

            var command = new ImportVocabularyFromJsonCommand
            {
                JsonData = request.JsonData
            };

            var result = await sender.Send(command);

            if (result.ErrorCount == 0)
            {
                return Results.Ok(new { 
                    success = true,
                    message = $"Successfully imported {result.SuccessCount} vocabulary words",
                    totalProcessed = result.TotalProcessed,
                    successCount = result.SuccessCount,
                    errorCount = result.ErrorCount
                });
            }
            else
            {
                return Results.BadRequest(new { 
                    success = false,
                    message = $"Imported {result.SuccessCount} words with {result.ErrorCount} errors",
                    totalProcessed = result.TotalProcessed,
                    successCount = result.SuccessCount,
                    errorCount = result.ErrorCount,
                    errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to import JSON: {ex.Message}");
        }
    }

    [Authorize]
    public Task<IResult> GetJsonTemplate()
    {
        var template = new[]
        {
            new {
                word = "hello",
                phonetic = "/həˈləʊ/",
                partOfSpeech = "interjection",
                meaning = "xin chào",
                definition = "used as a greeting or to begin a phone conversation",
                difficultyLevel = 1,
                categoryName = "Greetings"
            },
            new {
                word = "computer",
                phonetic = "/kəmˈpjuːtər/",
                partOfSpeech = "noun", 
                meaning = "máy tính",
                definition = "an electronic device for storing and processing data",
                difficultyLevel = 2,
                categoryName = "Technology"
            },
            new {
                word = "study",
                phonetic = "/ˈstʌdi/",
                partOfSpeech = "verb",
                meaning = "học, nghiên cứu", 
                definition = "to learn about something or acquire knowledge",
                difficultyLevel = 1,
                categoryName = "Education"
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(template, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        return Task.FromResult(Results.Text(json, "application/json", System.Text.Encoding.UTF8));
    }
}
