using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

namespace RoomEnglish.Application.VocabularyExamples.Commands.ImportExamplesFromWords;

public enum DifficultyLevel
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

public record ImportExamplesFromWordsCommand : IRequest<ImportExamplesWordsResult>
{
    public List<string> Words { get; init; } = new();
    public int ExampleCount { get; init; } = 10;
    public bool IncludeGrammar { get; init; } = true;
    public bool IncludeContext { get; init; } = true;
    public DifficultyLevel? DifficultyLevel { get; init; } = null;
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

        if (!request.Words.Any())
        {
            result.Errors.Add("No words provided");
            result.Message = "No words to process";
            return result;
        }

        try
        {
            // Get vocabulary words that exist in the database
            var existingVocabularyWords = await _context.VocabularyWords
                .Where(v => request.Words.Contains(v.Word) && v.IsActive)
                .ToListAsync(cancellationToken);

            if (!existingVocabularyWords.Any())
            {
                result.Errors.Add("No matching vocabulary words found in database");
                result.ErrorCount = request.Words.Count;
                result.Success = false;
                result.Message = "No vocabulary words found";
                return result;
            }

            // Process vocabulary words in batches with parallel ChatGPT calls
            await ProcessVocabularyWordsInBatches(existingVocabularyWords, result, request, cancellationToken);

            // Check for words that weren't found in the database
            var foundWords = existingVocabularyWords.Select(v => v.Word).ToHashSet();
            var notFoundWords = request.Words.Where(word => !foundWords.Contains(word)).ToList();
            
            foreach (var notFoundWord in notFoundWords)
            {
                result.Errors.Add($"Vocabulary word '{notFoundWord}' not found in database");
                result.ErrorCount++;
            }

            // Batch save all examples at once for better performance
            try
            {
                var savedCount = await _context.SaveChangesAsync(cancellationToken);
                result.Success = result.SuccessCount > 0;
                result.Message = result.Success 
                    ? $"Successfully generated {result.SuccessCount} examples for {existingVocabularyWords.Count} words with {savedCount} database changes. {result.ErrorCount} errors occurred."
                    : $"Failed to generate examples: {result.ErrorCount} errors";
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error saving to database: {ex.Message}");
                result.ErrorCount++;
                result.Success = false;
                result.Message = "Import failed during database save operation.";
            }

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

    private async Task ProcessVocabularyWordsInBatches(List<VocabularyWord> vocabularyWords, ImportExamplesWordsResult result, ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        // Get batch size from configuration or use default
        var batchSize = _configuration.GetValue<int>("ChatGPT:ConcurrentRequests", 5);
        var semaphore = new SemaphoreSlim(batchSize, batchSize);
        
        var tasks = vocabularyWords.Select(async word =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await ProcessVocabularyWord(word, result, request, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
    }

    private async Task ProcessVocabularyWord(VocabularyWord vocabularyWord, ImportExamplesWordsResult result, ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var examplesData = await GetExamplesDataFromChatGPT(vocabularyWord.Word, request);
            
            // Get existing examples for this word in one query
            var existingExamples = await _context.VocabularyExamples
                .Where(e => e.WordId == vocabularyWord.Id)
                .Select(e => e.Sentence.ToLower())
                .ToListAsync(cancellationToken);
            
            var examplesAddedCount = 0;
            var examplesSkippedCount = 0;
            
            foreach (var exampleData in examplesData)
            {
                try
                {
                    var sentencePrefix = exampleData.Sentence.ToLower().Substring(0, Math.Min(20, exampleData.Sentence.Length));
                    
                    // Check against in-memory list instead of database query
                    var isDuplicate = existingExamples.Any(existing => existing.Contains(sentencePrefix));

                    if (!isDuplicate)
                    {
                        var newExample = new VocabularyExample
                        {
                            Sentence = exampleData.Sentence,
                            Translation = exampleData.Translation,
                            Grammar = exampleData.Grammar,
                            WordId = vocabularyWord.Id,
                            IsActive = true,
                            DifficultyLevel = (int)(request.DifficultyLevel ?? Commands.ImportExamplesFromWords.DifficultyLevel.Easy),
                            DisplayOrder = 0
                        };

                        _context.VocabularyExamples.Add(newExample);
                        existingExamples.Add(exampleData.Sentence.ToLower()); // Add to in-memory cache
                        examplesAddedCount++;
                    }
                    else
                    {
                        examplesSkippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    lock (result.Errors)
                    {
                        result.Errors.Add($"Error processing example '{exampleData.Sentence}': {ex.Message}");
                        result.ErrorCount++;
                    }
                }
            }
            
            // Update counts in thread-safe manner
            lock (result)
            {
                result.SuccessCount += examplesAddedCount;
                result.ErrorCount += examplesSkippedCount;
                
                if (examplesSkippedCount > 0)
                {
                    result.Errors.Add($"Skipped {examplesSkippedCount} duplicate examples for word '{vocabularyWord.Word}'");
                }
            }

            // Don't save changes here - batch save will be done later
        }
        catch (Exception ex)
        {
            // If ChatGPT fails, use fallback mock data
            var fallbackExamples = GenerateFallbackExamples(vocabularyWord.Word, request);
            
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
                        DifficultyLevel = (int)(request.DifficultyLevel ?? Commands.ImportExamplesFromWords.DifficultyLevel.Easy),
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

    private async Task<List<ChatGPTExampleResponse>> GetExamplesDataFromChatGPT(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        var timeoutSeconds = _configuration.GetValue<int>("ChatGPT:RequestTimeoutSeconds", 30);
        var maxRetries = _configuration.GetValue<int>("ChatGPT:MaxRetries", 3);
        
        var client = new ChatClient("gpt-4o", apiKey);
        var prompt = CreatePromptForExamples(vocabularyWord, request);
        
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(prompt)
                };
                
                var chatCompletion = await client.CompleteChatAsync(messages, cancellationToken: cts.Token);
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
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt < maxRetries)
                {
                    // Exponential backoff: wait 1s, 2s, 4s...
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new InvalidOperationException($"ChatGPT failed after {maxRetries} attempts: {lastException?.Message}");
    }

    private List<ChatGPTExampleResponse> GenerateFallbackExamples(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var examples = new List<ChatGPTExampleResponse>();
        
        // Generate simple fallback examples for the vocabulary word
        for (int i = 1; i <= 3; i++)
        {
            examples.Add(new ChatGPTExampleResponse
            {
                Sentence = $"This is example {i} using the word '{vocabularyWord}' in context.",
                Translation = $"Đây là ví dụ {i} sử dụng từ '{vocabularyWord}' trong ngữ cảnh.",
                Grammar = $"Simple sentence structure demonstrating '{vocabularyWord}' usage."
            });
        }
        
        return examples;
    }

    private string CreatePromptForExamples(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var grammarInstruction = request.IncludeGrammar 
            ? "For each English sentence that uses a specific vocabulary word, analyze and explain the grammar used in a simple way.\r\n\r\nFocus on:\r\n- The grammatical role of the vocabulary word (e.g., noun, verb, object, etc.)\r\n- Sentence structure (e.g., S + V + O, passive voice, comparisons, etc.)\r\n- Any useful grammatical pattern used\r\n\r\nExplain in one short, clear sentence suitable for English learners. Output only the analysis.\r\n\r\nVocabulary word: \"computer\"\r\nSentence: \"I use my computer to write emails every morning.\"\r\n→ Grammar: \"'computer' là danh từ (noun) làm tân ngữ cho động từ 'use'; cấu trúc câu: S + V + O + to V\""
            : "Grammar explanations are optional.";
        
        var contextInstruction = request.IncludeContext 
            ? "Give examples in different contexts (formal, informal, business, everyday life, commonly used in life)."
            : "Use common everyday contexts.";
        
        var difficultyInstruction = request.DifficultyLevel switch
        {
            DifficultyLevel.Easy => "Create simple, easy-to-understand examples suitable for beginners.",
            DifficultyLevel.Medium => "Create examples with long, easy-to-understand sentences.",
            DifficultyLevel.Hard => "Create long, advanced examples with complex grammar and vocabulary, suitable for advanced learners.",
            _ => "Keep examples at beginner to intermediate level."
        };

        return $@"Create {request.ExampleCount} practical example sentences using the vocabulary word '{vocabularyWord}'.

        {contextInstruction}
        {difficultyInstruction}
        {grammarInstruction}

        Return ONLY a valid JSON array with this exact format:
        [
          {{
            ""Sentence"": ""English sentence using '{vocabularyWord}'"",
            ""Translation"": ""Natural Vietnamese translation"", 
            ""Grammar"": ""Brief grammar explanation (if applicable)""
          }}
        ]
        
        Requirements:
        - Each sentence must use the word '{vocabularyWord}' naturally
        - Sentences should be practical and commonly used
        - Vietnamese translations must be natural and accurate
        - Create exactly {request.ExampleCount} unique examples
        - Focus on demonstrating different uses of '{vocabularyWord}'
        - Avoid repetitive sentence structures
        ";
    }
}
