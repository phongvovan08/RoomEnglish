using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
    private readonly ILogger<ImportExamplesFromWordsCommandHandler> _logger;

    public ImportExamplesFromWordsCommandHandler(IApplicationDbContext context, IConfiguration configuration, ILogger<ImportExamplesFromWordsCommandHandler> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ImportExamplesWordsResult> Handle(ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var result = new ImportExamplesWordsResult
        {
            TotalProcessed = request.Words.Count
        };

        _logger.LogInformation("Starting example generation for {WordCount} words: {Words}", 
            request.Words.Count, string.Join(", ", request.Words));

        if (!request.Words.Any())
        {
            result.Errors.Add("No words provided");
            result.Message = "No words to process";
            return result;
        }

        try
        {
            // Get vocabulary words that exist in the database
            var dbQueryStopwatch = Stopwatch.StartNew();
            
            // Normalize words to lowercase for case-insensitive comparison
            var normalizedWords = request.Words.Select(w => w.ToLower()).ToList();
            
            var existingVocabularyWords = await _context.VocabularyWords
                .Where(v => normalizedWords.Contains(v.Word.ToLower()) && v.IsActive)
                .ToListAsync(cancellationToken);
            dbQueryStopwatch.Stop();

            _logger.LogInformation("Database query completed in {ElapsedMs}ms. Found {FoundCount}/{TotalCount} vocabulary words. Searched words: {SearchedWords}", 
                dbQueryStopwatch.ElapsedMilliseconds, existingVocabularyWords.Count, request.Words.Count, string.Join(", ", request.Words));

            if (!existingVocabularyWords.Any())
            {
                result.Errors.Add("No matching vocabulary words found in database");
                result.ErrorCount = request.Words.Count;
                result.Success = false;
                result.Message = "No vocabulary words found";
                _logger.LogWarning("No vocabulary words found for generation request");
                return result;
            }

            // Process vocabulary words in batches with parallel ChatGPT calls
            var processingStopwatch = Stopwatch.StartNew();
            await ProcessVocabularyWordsInBatches(existingVocabularyWords, result, request, cancellationToken);
            processingStopwatch.Stop();

            _logger.LogInformation("Parallel processing completed in {ElapsedMs}ms for {WordCount} words", 
                processingStopwatch.ElapsedMilliseconds, existingVocabularyWords.Count);

            // Check for words that weren't found in the database (case-insensitive)
            var foundWords = existingVocabularyWords.Select(v => v.Word.ToLower()).ToHashSet();
            var notFoundWords = request.Words
                .Where(word => !foundWords.Contains(word.ToLower()))
                .ToList();
            
            if (notFoundWords.Any())
            {
                _logger.LogWarning("Words not found in database: {NotFoundWords}", string.Join(", ", notFoundWords));
            }
            
            foreach (var notFoundWord in notFoundWords)
            {
                result.Errors.Add($"Vocabulary word '{notFoundWord}' not found in database. Please check spelling and case.");
                result.ErrorCount++;
            }

            // Batch save all examples at once for better performance
            var saveStopwatch = Stopwatch.StartNew();
            try
            {
                var savedCount = await _context.SaveChangesAsync(cancellationToken);
                saveStopwatch.Stop();
                totalStopwatch.Stop();

                result.Success = result.SuccessCount > 0;
                result.Message = result.Success 
                    ? $"Successfully generated {result.SuccessCount} examples for {existingVocabularyWords.Count} words with {savedCount} database changes. {result.ErrorCount} errors occurred."
                    : $"Failed to generate examples: {result.ErrorCount} errors";

                _logger.LogInformation("Database save completed in {SaveMs}ms. Total operation time: {TotalMs}ms. " +
                    "Generated {SuccessCount} examples with {ErrorCount} errors", 
                    saveStopwatch.ElapsedMilliseconds, totalStopwatch.ElapsedMilliseconds, 
                    result.SuccessCount, result.ErrorCount);
            }
            catch (Exception ex)
            {
                saveStopwatch.Stop();
                totalStopwatch.Stop();
                
                result.Errors.Add($"Error saving to database: {ex.Message}");
                result.ErrorCount++;
                result.Success = false;
                result.Message = "Import failed during database save operation.";
                
                _logger.LogError(ex, "Database save failed after {TotalMs}ms. Error: {ErrorMessage}", 
                    totalStopwatch.ElapsedMilliseconds, ex.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.ErrorCount++;
            result.Success = false;
            result.Message = "Processing failed";
            
            _logger.LogError(ex, "Unexpected error during example generation after {TotalMs}ms: {ErrorMessage}", 
                totalStopwatch.ElapsedMilliseconds, ex.Message);
            
            return result;
        }
    }

    private async Task ProcessVocabularyWordsInBatches(List<VocabularyWord> vocabularyWords, ImportExamplesWordsResult result, ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        // Get batch size from configuration or use default
        var batchSize = _configuration.GetValue<int>("ChatGPT:ConcurrentRequests", 5);
        var semaphore = new SemaphoreSlim(batchSize, batchSize);
        
        _logger.LogInformation("Starting parallel processing with batch size: {BatchSize} for {WordCount} words", 
            batchSize, vocabularyWords.Count);

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
        
        _logger.LogInformation("Parallel processing completed for all {WordCount} words", vocabularyWords.Count);
    }

    private async Task ProcessVocabularyWord(VocabularyWord vocabularyWord, ImportExamplesWordsResult result, ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        var wordStopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Starting processing for word: {Word}", vocabularyWord.Word);
        
        try
        {
            var chatGptStopwatch = Stopwatch.StartNew();
            var examplesData = await GetExamplesDataFromChatGPT(vocabularyWord.Word, request);
            chatGptStopwatch.Stop();
            
            _logger.LogDebug("ChatGPT API call completed for '{Word}' in {ElapsedMs}ms. Generated {ExampleCount} examples", 
                vocabularyWord.Word, chatGptStopwatch.ElapsedMilliseconds, examplesData.Count);
            
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

            wordStopwatch.Stop();
            _logger.LogDebug("Completed processing for '{Word}' in {ElapsedMs}ms. Added: {AddedCount}, Skipped: {SkippedCount}", 
                vocabularyWord.Word, wordStopwatch.ElapsedMilliseconds, examplesAddedCount, examplesSkippedCount);

            // Don't save changes here - batch save will be done later
        }
        catch (Exception ex)
        {
            wordStopwatch.Stop();
            _logger.LogError(ex, "Failed to generate examples for '{Word}' after {ElapsedMs}ms. Error: {ErrorMessage}", 
                vocabularyWord.Word, wordStopwatch.ElapsedMilliseconds, ex.Message);
            
            // Add detailed error message to the result
            var errorMessage = ex.InnerException != null 
                ? $"Failed to generate examples for '{vocabularyWord.Word}': {ex.Message} (Inner: {ex.InnerException.Message})"
                : $"Failed to generate examples for '{vocabularyWord.Word}': {ex.Message}";
            
            result.Errors.Add(errorMessage);
            result.ErrorCount++;
            
            // Don't create fallback examples - let the user know the actual error
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
        
        _logger.LogDebug("Starting ChatGPT API call for '{Word}' with {MaxRetries} max retries and {TimeoutSeconds}s timeout", 
            vocabularyWord, maxRetries, timeoutSeconds);
        
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var attemptStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("ChatGPT API attempt {Attempt}/{MaxRetries} for '{Word}'", attempt, maxRetries, vocabularyWord);
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(prompt)
                };
                
                var chatCompletion = await client.CompleteChatAsync(messages, cancellationToken: cts.Token);
                attemptStopwatch.Stop();
                
                _logger.LogDebug("ChatGPT API success on attempt {Attempt} for '{Word}' in {ElapsedMs}ms", 
                    attempt, vocabularyWord, attemptStopwatch.ElapsedMilliseconds);
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
                attemptStopwatch.Stop();
                lastException = ex;
                
                _logger.LogWarning("ChatGPT API attempt {Attempt}/{MaxRetries} failed for '{Word}' after {ElapsedMs}ms. Error: {ErrorMessage}", 
                    attempt, maxRetries, vocabularyWord, attemptStopwatch.ElapsedMilliseconds, ex.Message);
                
                if (attempt < maxRetries)
                {
                    // Exponential backoff: wait 1s, 2s, 4s...
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    _logger.LogDebug("Retrying ChatGPT API call for '{Word}' after {DelaySeconds}s delay", vocabularyWord, delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
        }
        
        _logger.LogError("ChatGPT API failed for '{Word}' after all {MaxRetries} attempts. Final error: {ErrorMessage}", 
            vocabularyWord, maxRetries, lastException?.Message);
        
        throw new InvalidOperationException($"ChatGPT failed after {maxRetries} attempts: {lastException?.Message}");
    }

    private string CreatePromptForExamples(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var grammarInstruction = request.IncludeGrammar 
            ? "For each English sentence that uses a specific vocabulary word, analyze and explain the grammar used in detail in Vietnamese.\r\n\r\nFocus on:\r\n- The grammatical role of the vocabulary word (e.g., noun, verb, object, etc.)\r\n- Sentence structure \r\n- Any useful grammatical pattern used\r\n\r\nExplain in one detail sentence suitable for English learners. Output only the analysis.\r\n\r\nVocabulary word: \"computer\"\r\nSentence: \"I use my computer to write emails every morning.\"\r\n→ Grammar: describe these sentences in detail"
            : "Grammar explanations are optional.";
        
        var contextInstruction = request.IncludeContext 
            ? "Give examples in different contexts (daily life, common usage in life, in meetings, in work, in IT, in software development)."
            : "Use common everyday contexts.";
        
        var difficultyInstruction = request.DifficultyLevel switch
        {
            DifficultyLevel.Easy => "Create simple, easy-to-understand examples suitable for beginners.",
            DifficultyLevel.Medium => "Create an example by combining 2 understandable sentences.",
            DifficultyLevel.Hard => "Create an example by combining 3 understandable sentences.",
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
            ""Grammar"": ""Describe these sentences in detail""
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
