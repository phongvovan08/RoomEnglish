using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomEnglish.Application.Vocabulary.Commands.CompleteLearningSession;
using RoomEnglish.Application.Vocabulary.Commands.ImportVocabularyFromExcel;
using RoomEnglish.Application.Vocabulary.Commands.StartLearningSession;
using RoomEnglish.Application.Vocabulary.Commands.SubmitDictation;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Web.Infrastructure;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace RoomEnglish.Web.Endpoints;

public class Vocabulary : EndpointGroupBase
{
    public override string? GroupName => "vocabulary-learning";
    
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization()
             .WithTags("Vocabulary Learning & Import/Export")
             .WithOpenApi();

        // Learning Position
        group.MapGet("learning-position/last", GetLastLearningPosition)
             .WithName("GetLastLearningPosition")
             .WithSummary("Get last learning position")
             .WithDescription("Gets user's most recent learning position across all words");

        // Learning Sessions
        group.MapPost("sessions/start", StartLearningSession)
             .WithName("StartLearningSession")
             .WithSummary("Start learning session")
             .WithDescription("Starts a new vocabulary learning session");
        group.MapPut("sessions/{id}/complete", CompleteLearningSession)
             .WithName("CompleteLearningSession")
             .WithSummary("Complete learning session")
             .WithDescription("Completes an active learning session");

        // Dictation
        group.MapPost("dictation/submit", SubmitDictation)
             .WithName("SubmitDictation")
             .WithSummary("Submit dictation")
             .WithDescription("Submits a dictation result for evaluation");

        // Excel Upload/Export
        group.MapPost("upload-excel", UploadExcel)
             .WithName("UploadVocabularyExcel")
             .WithSummary("Upload Excel file")
           .WithDescription("Imports vocabulary data from an Excel file");
        group.MapGet("export-excel", ExportExcel)
             .WithName("ExportVocabularyExcel")
             .WithSummary("Export to Excel")
             .WithDescription("Exports vocabulary data to an Excel file");
        group.MapGet("template.xlsx", GetExcelTemplate)
             .WithName("GetVocabularyTemplate")
             .WithSummary("Download Excel template")
             .WithDescription("Downloads an Excel template for vocabulary import");
    }

    [Authorize]
    public async Task<IResult> GetLastLearningPosition(ISender sender)
    {
        var result = await sender.Send(new GetLastLearningPositionQuery());
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

    [Authorize]
    public async Task<IResult> UploadExcel(
        HttpRequest httpRequest,
        ISender sender)
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

        using var stream = file.OpenReadStream();
        var command = new ImportVocabularyFromExcelCommand
        {
            ExcelStream = stream,
            FileName = file.FileName
        };

        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    [Authorize]
    public async Task<IResult> ExportExcel(
        ISender sender,
        IApplicationDbContext context)
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vocabulary");

            // Add headers
            var headers = new[] { 
                "Word", "Phonetic", "PartOfSpeech", "Meaning", "Definition", 
                "DifficultyLevel", "Category", "ExampleSentence", "ExampleTranslation", "ExampleGrammar" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Get vocabulary data
            var words = await context.VocabularyWords
                .Include(w => w.Category)
                .Include(w => w.Examples)
                .Where(w => w.IsActive)
                .ToListAsync();

            int row = 2;
            foreach (var word in words)
            {
                worksheet.Cells[row, 1].Value = word.Word;
                worksheet.Cells[row, 2].Value = word.Phonetic;
                worksheet.Cells[row, 3].Value = word.PartOfSpeech;
                worksheet.Cells[row, 4].Value = word.Meaning;
                worksheet.Cells[row, 5].Value = word.Definition;
                worksheet.Cells[row, 6].Value = word.DifficultyLevel;
                worksheet.Cells[row, 7].Value = word.Category?.Name ?? "General";

                // Add first example if available
                var firstExample = word.Examples.FirstOrDefault();
                if (firstExample != null)
                {
                    worksheet.Cells[row, 8].Value = firstExample.Sentence;
                    worksheet.Cells[row, 9].Value = firstExample.Translation;
                    worksheet.Cells[row, 10].Value = firstExample.Grammar;
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            return Results.File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"vocabulary-export-{DateTime.Now:yyyy-MM-dd}.xlsx"
            );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Export failed: {ex.Message}");
        }
    }

    public async Task<IResult> GetExcelTemplate()
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("VocabularyTemplate");

            // Add headers
            var headers = new[] { 
                "Word", "Phonetic", "PartOfSpeech", "Meaning", "Definition", 
                "DifficultyLevel", "Category", "ExampleSentence", "ExampleTranslation", "ExampleGrammar" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Add sample data
            var sampleData = new object[,]
            {
                { "hello", "/həˈloʊ/", "interjection", "xin chào", "used as a greeting", 1, "Greetings", "Hello, how are you?", "Xin chào, bạn khỏe không?", "Basic greeting expression" },
                { "beautiful", "/ˈbjuːtɪfəl/", "adjective", "đẹp", "pleasing the senses or mind aesthetically", 2, "Appearance", "She is a beautiful woman.", "Cô ấy là một người phụ nữ đẹp.", "Adjective describing physical appearance" },
                { "study", "/ˈstʌdi/", "verb", "học", "to learn about something", 1, "Education", "I study English every day.", "Tôi học tiếng Anh mỗi ngày.", "Present simple tense with frequency adverb" }
            };

            for (int i = 0; i < sampleData.GetLength(0); i++)
            {
                for (int j = 0; j < sampleData.GetLength(1); j++)
                {
                    worksheet.Cells[i + 2, j + 1].Value = sampleData[i, j];
                }
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Add data validation for difficulty level
            var difficultyRange = worksheet.Cells[2, 6, 1000, 6];
            var validation = difficultyRange.DataValidation.AddListDataValidation();
            validation.Formula.Values.Add("1");
            validation.Formula.Values.Add("2");
            validation.Formula.Values.Add("3");
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Invalid Difficulty";
            validation.Error = "Please enter 1, 2, or 3 for difficulty level";

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            return Results.File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "vocabulary_template.xlsx"
            );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Template generation failed: {ex.Message}");
        }
    }
}

// Request models
public class CompleteLearningSessionRequest
{
    public int CorrectAnswers { get; set; }
    public int Score { get; set; }
}
