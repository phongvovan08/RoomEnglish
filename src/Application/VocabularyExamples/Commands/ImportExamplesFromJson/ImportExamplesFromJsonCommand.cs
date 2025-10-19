using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromJson;

public record ImportExamplesFromJsonCommand : IRequest<ImportExamplesJsonResult>
{
    public string JsonData { get; init; } = string.Empty;
    public int VocabularyId { get; init; }
}

public class ImportExamplesJsonResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class JsonExampleData
{
    public string sentence { get; set; } = string.Empty;
    public string translation { get; set; } = string.Empty;
    public string grammar { get; set; } = string.Empty;
}

public class ImportExamplesFromJsonCommandHandler : IRequestHandler<ImportExamplesFromJsonCommand, ImportExamplesJsonResult>
{
    private readonly IApplicationDbContext _context;

    public ImportExamplesFromJsonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportExamplesJsonResult> Handle(ImportExamplesFromJsonCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportExamplesJsonResult();

        try
        {
            // Validate JSON data
            if (string.IsNullOrWhiteSpace(request.JsonData))
            {
                result.Errors.Add("JSON data cannot be empty");
                result.ErrorCount = 1;
                result.Success = false;
                result.Message = "Invalid input";
                return result;
            }

            // Get vocabulary word to ensure it exists
            var vocabularyWord = await _context.VocabularyWords
                .FirstOrDefaultAsync(v => v.Id == request.VocabularyId && v.IsActive, cancellationToken);
            
            if (vocabularyWord == null)
            {
                result.Errors.Add($"Vocabulary with ID {request.VocabularyId} not found or inactive");
                result.ErrorCount = 1;
                result.Success = false;
                result.Message = "Vocabulary not found";
                return result;
            }

            // Parse JSON data
            List<JsonExampleData> examplesData;
            try
            {
                examplesData = JsonSerializer.Deserialize<List<JsonExampleData>>(request.JsonData) ?? new List<JsonExampleData>();
            }
            catch (JsonException ex)
            {
                result.Errors.Add($"Invalid JSON format: {ex.Message}");
                result.ErrorCount = 1;
                result.Success = false;
                result.Message = "JSON parsing failed";
                return result;
            }

            result.TotalProcessed = examplesData.Count;

            if (examplesData.Count == 0)
            {
                result.Errors.Add("No examples found in JSON data");
                result.ErrorCount = 1;
                result.Success = false;
                result.Message = "No data to process";
                return result;
            }

            // Process each example
            foreach (var exampleData in examplesData)
            {
                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(exampleData.sentence))
                    {
                        result.Errors.Add("Example missing required 'sentence' field");
                        result.ErrorCount++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(exampleData.translation))
                    {
                        result.Errors.Add($"Example '{exampleData.sentence}' missing required 'translation' field");
                        result.ErrorCount++;
                        continue;
                    }

                    // Check if example already exists
                    var existingExample = await _context.VocabularyExamples
                        .FirstOrDefaultAsync(e => e.WordId == request.VocabularyId && 
                                                e.Sentence.ToLower() == exampleData.sentence.ToLower(), 
                                           cancellationToken);

                    if (existingExample != null)
                    {
                        result.Errors.Add($"Example already exists: {exampleData.sentence}");
                        result.ErrorCount++;
                        continue;
                    }

                    // Create new example
                    var newExample = new VocabularyExample
                    {
                        Sentence = exampleData.sentence.Trim(),
                        Translation = exampleData.translation.Trim(),
                        Grammar = exampleData.grammar?.Trim() ?? string.Empty,
                        WordId = request.VocabularyId,
                        IsActive = true,
                        DifficultyLevel = 1, // Default difficulty
                        DisplayOrder = 0
                    };

                    _context.VocabularyExamples.Add(newExample);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing example '{exampleData.sentence}': {ex.Message}");
                    result.ErrorCount++;
                }
            }

            // Save changes
            if (result.SuccessCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            result.Success = result.ErrorCount == 0;
            result.Message = result.Success 
                ? $"Successfully imported {result.SuccessCount} examples"
                : $"Imported {result.SuccessCount} examples with {result.ErrorCount} errors";

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.ErrorCount++;
            result.Success = false;
            result.Message = "Import failed";
            return result;
        }
    }
}