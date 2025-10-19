using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromWords;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Application.Vocabulary.Commands.ImportVocabularyFromWords;

public record ImportVocabularyFromWordsCommand : IRequest<ImportVocabularyWordsResult>
{
    public List<string> Words { get; init; } = new();
}

public class ImportVocabularyWordsResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ChatGPTVocabularyResponse
{
    public string Word { get; set; } = string.Empty;
    public string Phonetic { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class ImportVocabularyFromWordsCommandHandler : IRequestHandler<ImportVocabularyFromWordsCommand, ImportVocabularyWordsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public ImportVocabularyFromWordsCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ImportVocabularyWordsResult> Handle(ImportVocabularyFromWordsCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportVocabularyWordsResult
        {
            TotalProcessed = request.Words.Count
        };

        if (!request.Words.Any())
        {
            result.Errors.Add("No words provided");
            result.Message = "No words to process";
            return result;
        }

        var existingWords = await _context.VocabularyWords
            .Where(v => request.Words.Contains(v.Word))
            .Select(v => v.Word.ToLower())
            .ToListAsync(cancellationToken);

        var wordsToProcess = request.Words
            .Where(word => !existingWords.Contains(word.ToLower()))
            .ToList();

        if (wordsToProcess.Count == 0)
        {
            result.Errors.AddRange(request.Words.Select(word => $"Word '{word}' already exists"));
            result.ErrorCount = request.Words.Count;
            result.Message = $"All {request.Words.Count} words already exist";
            return result;
        }

        // Get vocabulary data from ChatGPT in batches
        var vocabularyDataList = await GetVocabularyDataFromChatGPTInBatches(wordsToProcess);

        // Get default category ID (assuming we have a default category)
        var categoryDataList = await _context.VocabularyCategories
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        if (!categoryDataList.Any())
        {
            result.Errors.Add("No active vocabulary category found");
            result.ErrorCount = request.Words.Count;
            result.Message = "No category available for import";
            return result;
        }
        var categoryDict = categoryDataList.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var vocabData in vocabularyDataList)
        {
            try
            {
                if (!categoryDict.TryGetValue(vocabData.CategoryName, out var categoryId))
                {
                    result.Errors.Add($"Category not found: {vocabData.CategoryName}");
                    result.ErrorCount++;
                    continue;
                }
                var vocabularyWord = new VocabularyWord
                {
                    Word = vocabData.Word,
                    Phonetic = vocabData.Phonetic,
                    PartOfSpeech = vocabData.PartOfSpeech,
                    Meaning = vocabData.Meaning,
                    Definition = vocabData.Definition,
                    CategoryId = categoryId,
                    DifficultyLevel = 1, // Easy
                    IsActive = true,
                    ViewCount = 0,
                    CorrectCount = 0,
                    IncorrectCount = 0
                };

                _context.VocabularyWords.Add(vocabularyWord);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process word '{vocabData.Word}': {ex.Message}");
                result.ErrorCount++;
            }
        }

        // Add errors for words that already exist
        var duplicateWords = request.Words.Where(word => existingWords.Contains(word.ToLower()));
        foreach (var word in duplicateWords)
        {
            result.Errors.Add($"Word '{word}' already exists");
            result.ErrorCount++;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            
            result.Success = result.SuccessCount > 0;
            result.Message = result.ErrorCount > 0 
                ? $"Imported {result.SuccessCount} words with {result.ErrorCount} errors"
                : $"Successfully imported {result.SuccessCount} words";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Database save error: {ex.Message}");
            result.ErrorCount = request.Words.Count;
            result.SuccessCount = 0;
            result.Message = "Failed to save to database";
        }

        return result;
    }

    private async Task<List<ChatGPTVocabularyResponse>> GetVocabularyDataFromChatGPTInBatches(List<string> words)
    {
        const int batchSize = 50;
        var tasks = new List<Task<List<ChatGPTVocabularyResponse>>>();

        for (int i = 0; i < words.Count; i += batchSize)
        {
            var batch = words.Skip(i).Take(batchSize).ToList();
            tasks.Add(GetVocabularyDataFromChatGPT(batch));
        }

        var batchResults = await Task.WhenAll(tasks);
        return batchResults.SelectMany(r => r).ToList();
    }

    private async Task<List<ChatGPTVocabularyResponse>> GetVocabularyDataFromChatGPT(List<string> words)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Fallback to mock data if API key is not configured
                return GetMockVocabularyData(words);
            }

            var client = new ChatClient("gpt-4o", apiKey);
            
            var prompt = CreatePromptForWords(words);
            var response = await client.CompleteChatAsync(prompt);
            
            var content = response.Value.Content[0].Text;
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                return JsonSerializer.Deserialize<List<ChatGPTVocabularyResponse>>(jsonContent) ?? new List<ChatGPTVocabularyResponse>();
            }
            throw new InvalidOperationException("Invalid JSON response from ChatGPT");
        }
        catch (Exception ex)
        {
            // Log error and return mock data as fallback
            Console.WriteLine($"ChatGPT API Error: {ex.Message}");
            return GetMockVocabularyData(words);
        }
    }

    private List<ChatGPTVocabularyResponse> GetMockVocabularyData(List<string> words)
    {
        var mockResults = new List<ChatGPTVocabularyResponse>();

        foreach (var word in words)
        {
            mockResults.Add(new ChatGPTVocabularyResponse
            {
                Word = word,
                Phonetic = $"/{word}/", // Mock phonetic
                PartOfSpeech = "noun", // Mock part of speech
                Meaning = $"Nghĩa của từ {word}", // Mock Vietnamese meaning
                Definition = $"Definition of {word} in English" // Mock English definition
            });
        }

        return mockResults;
    }

    private string CreatePromptForWords(List<string> words)
    {
        return $@"
For each of these English words: {string.Join(", ", words)}, 
Each object must include:
- Word (Only English vocabulary)
- Phonetic (IPA format: /phonetic/)
- PartOfSpeech (string)
- Meaning (some meanings used)
- Definition (English)
- CategoryName (level of Word)
Return ONLY a valid JSON array with this exact format:
[
  {{
    ""Word"": ""Hello"",
    ""Phonetic"": ""/həˈloʊ/"",
    ""PartOfSpeech"": ""Interjection"",
    ""Meaning"": ""Xin chào"",
    ""Definition"": ""Used as a greeting or to begin a phone conversation""
    ""CategoryName"": ""A1""
  }}
]";
    }


}
