using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

namespace RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromWords;

public record ImportExamplesFromWordsCommand : IRequest<ImportExamplesWordsResult>
{
    public List<string> Words { get; init; } = new();
    public int VocabularyId { get; init; }
}

public class ImportExamplesWordsResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ChatGPTExampleResponse
{
    public string Sentence { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Grammar { get; set; } = string.Empty;
}

public class ImportExamplesFromWordsCommandHandler : IRequestHandler<ImportExamplesFromWordsCommand, ImportExamplesWordsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public ImportExamplesFromWordsCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ImportExamplesWordsResult> Handle(ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportExamplesWordsResult
        {
            TotalProcessed = request.Words.Count
        };

        try
        {
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

            // Process words in batches for better performance
            var batchSize = 5; // Process 5 words at a time
            var totalWords = request.Words.Count;
            
            for (int i = 0; i < totalWords; i += batchSize)
            {
                var batch = request.Words.Skip(i).Take(batchSize).ToList();
                await ProcessWordsBatch(batch, vocabularyWord, result, cancellationToken);
            }

            result.Success = result.ErrorCount == 0;
            result.Message = result.Success 
                ? $"Successfully processed {result.SuccessCount} examples"
                : $"Processed with {result.ErrorCount} errors";

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.ErrorCount++;
            result.Success = false;
            result.Message = "Processing failed";
            return result;
        }
    }

    private async Task ProcessWordsBatch(List<string> words, VocabularyWord vocabularyWord, ImportExamplesWordsResult result, CancellationToken cancellationToken)
    {
        try
        {
            var examplesData = await GetExamplesDataFromChatGPT(words, vocabularyWord.Word);
            
            foreach (var exampleData in examplesData)
            {
                try
                {
                    // Check if similar example already exists
                    var existingExample = await _context.VocabularyExamples
                        .FirstOrDefaultAsync(e => e.WordId == vocabularyWord.Id && 
                                                e.Sentence.ToLower().Contains(exampleData.Sentence.ToLower().Substring(0, Math.Min(20, exampleData.Sentence.Length))), 
                                           cancellationToken);

                    if (existingExample == null)
                    {
                        var newExample = new VocabularyExample
                        {
                            Sentence = exampleData.Sentence,
                            Translation = exampleData.Translation,
                            Grammar = exampleData.Grammar,
                            WordId = vocabularyWord.Id,
                            IsActive = true,
                            DifficultyLevel = 1, // Default difficulty
                            DisplayOrder = 0
                        };

                        _context.VocabularyExamples.Add(newExample);
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.Errors.Add($"Similar example already exists for: {exampleData.Sentence.Substring(0, Math.Min(30, exampleData.Sentence.Length))}...");
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing example '{exampleData.Sentence}': {ex.Message}");
                    result.ErrorCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // If ChatGPT fails, use fallback mock data
            var fallbackExamples = GenerateFallbackExamples(words, vocabularyWord.Word);
            
            foreach (var example in fallbackExamples)
            {
                try
                {
                    var newExample = new VocabularyExample
                    {
                        Sentence = example.Sentence,
                        Translation = example.Translation,
                        Grammar = example.Grammar,
                        WordId = vocabularyWord.Id,
                        IsActive = true,
                        DifficultyLevel = 1,
                        DisplayOrder = 0
                    };

                    _context.VocabularyExamples.Add(newExample);
                    result.SuccessCount++;
                }
                catch (Exception fallbackEx)
                {
                    result.Errors.Add($"Error with fallback data: {fallbackEx.Message}");
                    result.ErrorCount++;
                }
            }
            
            result.Errors.Add($"ChatGPT failed, used fallback data: {ex.Message}");
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<List<ChatGPTExampleResponse>> GetExamplesDataFromChatGPT(List<string> words, string mainVocabulary)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        var client = new ChatClient("gpt-3.5-turbo", apiKey);
        
        var wordsText = string.Join(", ", words);
        var prompt = $@"Create example sentences using these words: {wordsText}. 
        The main vocabulary word is '{mainVocabulary}'. 
        Create 1-2 practical example sentences for each word that demonstrate proper usage.
        
        Return ONLY a valid JSON array with this exact format:
        [
          {{
            ""sentence"": ""English sentence using the word"",
            ""translation"": ""Vietnamese translation"", 
            ""grammar"": ""Brief grammar explanation""
          }}
        ]
        
        Requirements:
        - Each sentence should be practical and commonly used
        - Translations should be natural Vietnamese
        - Grammar explanations should be brief and helpful
        - Focus on the main vocabulary word '{mainVocabulary}' but include other words naturally
        - Maximum 2 examples per word provided
        ";

        var chatCompletion = await client.CompleteChatAsync(prompt);
        var content = chatCompletion.Value.Content[0].Text;
        
        // Clean up the response to ensure valid JSON
        var jsonStart = content.IndexOf('[');
        var jsonEnd = content.LastIndexOf(']');
        
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
            return JsonSerializer.Deserialize<List<ChatGPTExampleResponse>>(jsonContent) ?? new List<ChatGPTExampleResponse>();
        }
        
        throw new InvalidOperationException("Invalid JSON response from ChatGPT");
    }

    private List<ChatGPTExampleResponse> GenerateFallbackExamples(List<string> words, string mainVocabulary)
    {
        var examples = new List<ChatGPTExampleResponse>();
        
        foreach (var word in words.Take(3)) // Limit fallback examples
        {
            examples.Add(new ChatGPTExampleResponse
            {
                Sentence = $"This is an example sentence using the word '{word}' with {mainVocabulary}.",
                Translation = $"Đây là câu ví dụ sử dụng từ '{word}' với {mainVocabulary}.",
                Grammar = $"Simple sentence structure demonstrating '{word}' usage."
            });
        }
        
        return examples;
    }
}