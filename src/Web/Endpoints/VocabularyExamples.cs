using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;
using RoomEnglish.Application.Vocabulary.Queries.GetVocabularyExamples;
using RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromJson;
using RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromWords;
using OfficeOpenXml;
using System.Text.Json;

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
        group.MapPost("upload-excel", UploadExcel)
             .WithName("UploadVocabularyExamples")
             .WithSummary("Upload examples from Excel")
             .WithDescription("Imports vocabulary examples from an Excel file");
        group.MapGet("template.xlsx", GetExcelTemplate)
             .WithName("GetVocabularyExamplesTemplate")
             .WithSummary("Download Excel template")
             .WithDescription("Downloads an Excel template for vocabulary examples import");
        group.MapPost("import-json", ImportJson)
             .WithName("ImportVocabularyExamplesFromJson")
             .WithSummary("Import examples from JSON")
             .WithDescription("Imports vocabulary examples from JSON data");
        group.MapPost("import-words", ImportWords)
             .WithName("ImportVocabularyExamplesFromWords")
             .WithSummary("Import examples from word list")
             .WithDescription("Generates vocabulary examples from word list using ChatGPT");
        group.MapGet("template.json", GetJsonTemplate)
             .WithName("GetVocabularyExamplesJsonTemplate")
             .WithSummary("Download JSON template")
             .WithDescription("Downloads a JSON template for vocabulary examples import");
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

    [Authorize]
    public async Task<IResult> UploadExcel(
        HttpRequest httpRequest,
        IApplicationDbContext context)
    {
        if (!httpRequest.HasFormContentType)
        {
            return Results.BadRequest("Invalid content type");
        }

        var form = await httpRequest.ReadFormAsync();
        var file = form.Files["file"];
        var vocabularyIdStr = form["vocabularyId"];

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded");
        }

        if (!int.TryParse(vocabularyIdStr, out var vocabularyId))
        {
            return Results.BadRequest("Invalid vocabulary ID");
        }

        // Verify vocabulary exists
        var vocabularyExists = await context.VocabularyWords
            .AnyAsync(v => v.Id == vocabularyId && v.IsActive);
        if (!vocabularyExists)
        {
            return Results.BadRequest("Vocabulary word not found");
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return Results.BadRequest("No worksheet found in Excel file");
            }

            var addedCount = 0;
            var updatedCount = 0;
            var errors = new List<string>();

            // Expected columns: Sentence (A), Translation (B), Grammar (C)
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var sentence = worksheet.Cells[row, 1].Text?.Trim();
                    var translation = worksheet.Cells[row, 2].Text?.Trim();
                    var grammar = worksheet.Cells[row, 3].Text?.Trim();

                    if (string.IsNullOrEmpty(sentence) || string.IsNullOrEmpty(translation))
                    {
                        errors.Add($"Dòng {row}: Thiếu câu ví dụ hoặc bản dịch");
                        continue;
                    }

                    // Check if example already exists for this vocabulary word
                    var existingExample = await context.VocabularyExamples
                        .FirstOrDefaultAsync(e => e.WordId == vocabularyId && 
                                                  e.Sentence == sentence);

                    if (existingExample != null)
                    {
                        // Update existing example
                        existingExample.Translation = translation;
                        existingExample.Grammar = grammar ?? string.Empty;
                        existingExample.IsActive = true;
                        updatedCount++;
                    }
                    else
                    {
                        // Create new example
                        var newExample = new RoomEnglish.Domain.Entities.VocabularyExample
                        {
                            Sentence = sentence,
                            Translation = translation,
                            Grammar = grammar ?? string.Empty,
                            WordId = vocabularyId,
                            IsActive = true,
                            DifficultyLevel = 1,
                            DisplayOrder = 0
                        };

                        context.VocabularyExamples.Add(newExample);
                        addedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Dòng {row}: {ex.Message}");
                }
            }

            await context.SaveChangesAsync(CancellationToken.None);

            return Results.Ok(new
            {
                success = true,
                addedCount,
                updatedCount,
                errors
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error processing Excel file: {ex.Message}");
        }
    }

    public async Task<IResult> GetExcelTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Examples Template");

        // Headers
        worksheet.Cells[1, 1].Value = "Sentence (English)";
        worksheet.Cells[1, 2].Value = "Translation (Vietnamese)";
        worksheet.Cells[1, 3].Value = "Grammar Explanation";

        // Sample data
        var sampleData = new object[,]
        {
            { "Hello, how are you?", "Xin chào, bạn khỏe không?", "Basic greeting expression" },
            { "She is a beautiful woman.", "Cô ấy là một người phụ nữ đẹp.", "Adjective describing physical appearance" },
            { "I love reading books.", "Tôi thích đọc sách.", "Simple present tense expressing preference" }
        };

        for (int i = 0; i < sampleData.GetLength(0); i++)
        {
            for (int j = 0; j < sampleData.GetLength(1); j++)
            {
                worksheet.Cells[i + 2, j + 1].Value = sampleData[i, j];
            }
        }

        // Format headers
        using (var range = worksheet.Cells[1, 1, 1, 3])
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
            "vocabulary-examples-template.xlsx");
    }

    public record ImportJsonRequest(string JsonData, int VocabularyId);
    public record ImportWordsRequest(string[] Words, int VocabularyId);

    [Authorize]
    public async Task<IResult> ImportJson(
        ISender sender,
        ImportJsonRequest request)
    {
        var command = new ImportExamplesFromJsonCommand 
        { 
            JsonData = request.JsonData,
            VocabularyId = request.VocabularyId
        };
        
        var result = await sender.Send(command);
        
        return Results.Ok(new 
        { 
            successCount = result.SuccessCount, 
            errorCount = result.ErrorCount, 
            errors = result.Errors.ToArray(),
            success = result.Success,
            message = result.Message
        });
    }

    [Authorize]
    public async Task<IResult> ImportWords(
        ISender sender,
        ImportWordsRequest request)
    {
        var command = new ImportExamplesFromWordsCommand 
        { 
            Words = request.Words.ToList(),
            VocabularyId = request.VocabularyId
        };
        
        var result = await sender.Send(command);
        
        return Results.Ok(new 
        { 
            successCount = result.SuccessCount, 
            errorCount = result.ErrorCount, 
            errors = result.Errors.ToArray(),
            success = result.Success,
            message = result.Message
        });
    }

    [Authorize]
    public IResult GetJsonTemplate()
    {
        var template = new[]
        {
            new
            {
                sentence = "This is an example sentence",
                translation = "Đây là câu ví dụ",
                grammar = "Present simple tense"
            }
        };

        var jsonString = System.Text.Json.JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        return Results.File(bytes, "application/json", "examples-template.json");
    }
}
