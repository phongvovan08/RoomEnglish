using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace RoomEnglish.Application.Vocabulary.Commands.ImportVocabularyFromJson;

public record ImportVocabularyFromJsonCommand : IRequest<ImportVocabularyJsonResult>
{
    public string JsonData { get; init; } = string.Empty;
}

public class ImportVocabularyJsonResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class VocabularyWordJsonDto
{
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string? PartOfSpeech { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public int DifficultyLevel { get; set; } = 1;
    public string? CategoryName { get; set; }
}

public class ImportVocabularyFromJsonCommandHandler : IRequestHandler<ImportVocabularyFromJsonCommand, ImportVocabularyJsonResult>
{
    private readonly IApplicationDbContext _context;

    public ImportVocabularyFromJsonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportVocabularyJsonResult> Handle(ImportVocabularyFromJsonCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportVocabularyJsonResult();

        try
        {
            var vocabularyList = JsonSerializer.Deserialize<List<VocabularyWordJsonDto>>(request.JsonData, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (vocabularyList == null || !vocabularyList.Any())
            {
                result.Errors.Add("JSON data is empty or invalid format");
                return result;
            }

            result.TotalProcessed = vocabularyList.Count;

            // Get all categories to cache lookups
            var categories = await _context.VocabularyCategories
                .Where(c => c.IsActive)
                .ToDictionaryAsync(c => c.Name.ToLower(), c => c, cancellationToken);

            foreach (var vocabDto in vocabularyList)
            {
                try
                {
                    await ProcessVocabularyWord(vocabDto, categories, result, cancellationToken);
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Error processing word '{vocabDto.Word}': {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Invalid JSON format: {ex.Message}");
            result.ErrorCount = result.TotalProcessed;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message} {ex.InnerException?.Message}");
            result.ErrorCount = result.TotalProcessed;
        }

        return result;
    }

    private async Task ProcessVocabularyWord(
        VocabularyWordJsonDto vocabDto, 
        Dictionary<string, VocabularyCategory> categories,
        ImportVocabularyJsonResult result, 
        CancellationToken cancellationToken)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(vocabDto.Word))
        {
            result.ErrorCount++;
            result.Errors.Add("Word is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(vocabDto.Meaning))
        {
            result.ErrorCount++;
            result.Errors.Add($"Meaning is required for word '{vocabDto.Word}'");
            return;
        }

        // Check if word already exists
        var existingWord = await _context.VocabularyWords
            .FirstOrDefaultAsync(w => w.Word.ToLower() == vocabDto.Word.ToLower(), cancellationToken);
        
        if (existingWord != null)
        {
            result.ErrorCount++;
            result.Errors.Add($"Word '{vocabDto.Word}' already exists");
            return;
        }

        // Get or create category
        VocabularyCategory? category = null;
        if (!string.IsNullOrWhiteSpace(vocabDto.CategoryName))
        {
            var categoryName = vocabDto.CategoryName.ToLower();
            if (categories.TryGetValue(categoryName, out category))
            {
                // Category exists
            }
            else
            {
                // Create new category
                category = new VocabularyCategory
                {
                    Name = vocabDto.CategoryName.Trim(),
                    Description = $"Auto-created from JSON import",
                    IsActive = true
                };
                _context.VocabularyCategories.Add(category);
                categories[categoryName] = category;
            }
        }

        // Create vocabulary word
        var vocabularyWord = new VocabularyWord
        {
            Word = vocabDto.Word.Trim(),
            Phonetic = vocabDto.Phonetic?.Trim() ?? string.Empty,
            PartOfSpeech = vocabDto.PartOfSpeech?.Trim() ?? string.Empty,
            Meaning = vocabDto.Meaning.Trim(),
            Definition = vocabDto.Definition?.Trim() ?? string.Empty,
            DifficultyLevel = Math.Max(1, Math.Min(3, vocabDto.DifficultyLevel)), // Ensure 1-3 range
            IsActive = true,
            ViewCount = 0,
            CorrectCount = 0,
            IncorrectCount = 0
        };

        if (category != null)
        {
            vocabularyWord.CategoryId = category.Id;
        }

        _context.VocabularyWords.Add(vocabularyWord);

        result.SuccessCount++;
    }
}
