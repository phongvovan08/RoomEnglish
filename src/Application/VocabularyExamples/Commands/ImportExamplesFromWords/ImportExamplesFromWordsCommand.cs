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
    public List<DifficultyLevel>? DifficultyLevels { get; init; } = null;
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
    public string? Phonetic { get; set; }
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

    private async Task ProcessVocabularyWordsInBatches(
    List<VocabularyWord> vocabularyWords,
    ImportExamplesWordsResult result,
    ImportExamplesFromWordsCommand request,
    CancellationToken cancellationToken)
    {
        var batchSize = _configuration.GetValue<int>("ChatGPT:ConcurrentRequests", 10);
        var semaphore = new SemaphoreSlim(batchSize, batchSize);
        var allNewExamples = new List<VocabularyExample>();
        var lockObject = new object();

        _logger.LogInformation("Starting parallel ChatGPT calls with max {MaxDegree} concurrent requests for {WordCount} words",
            batchSize, vocabularyWords.Count);

        // Phase 1: Call ChatGPT in parallel to get all examples
        var tasks = vocabularyWords.Select(async word =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var examples = await ProcessVocabularyWordGetExamples(word, result, request, cancellationToken);
                
                // Add to collection in thread-safe manner
                if (examples.Any())
                {
                    lock (lockObject)
                    {
                        allNewExamples.AddRange(examples);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing word '{Word}': {ErrorMessage}", word.Word, ex.Message);
                lock (result)
                {
                    result.Errors.Add($"Error processing word '{word.Word}': {ex.Message}");
                    result.ErrorCount++;
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation("Completed parallel ChatGPT calls. Collected {TotalExamples} examples from {WordCount} words",
            allNewExamples.Count, vocabularyWords.Count);

        // Phase 2: Save all examples to database in one batch
        if (allNewExamples.Any())
        {
            _logger.LogInformation("Saving {Count} examples to database...", allNewExamples.Count);
            
            _context.VocabularyExamples.AddRange(allNewExamples);

            _logger.LogInformation("All examples added to context. Ready for SaveChanges.");
        }
    }

    private async Task<List<VocabularyExample>> ProcessVocabularyWordGetExamples(
        VocabularyWord vocabularyWord, 
        ImportExamplesWordsResult result, 
        ImportExamplesFromWordsCommand request, 
        CancellationToken cancellationToken)
    {
        var wordStopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Starting ChatGPT call for word: {Word}", vocabularyWord.Word);
        
        try
        {
            // Call ChatGPT to get examples
            var chatGptStopwatch = Stopwatch.StartNew();
            var examplesData = await GetExamplesDataFromChatGPT(vocabularyWord.Word, request);
            chatGptStopwatch.Stop();
            
            _logger.LogDebug("ChatGPT API call completed for '{Word}' in {ElapsedMs}ms. Generated {ExampleCount} examples", 
                vocabularyWord.Word, chatGptStopwatch.ElapsedMilliseconds, examplesData.Count);
            
            if (examplesData == null || !examplesData.Any())
            {
                _logger.LogWarning("No examples returned from ChatGPT for word '{Word}'", vocabularyWord.Word);
                lock (result)
                {
                    result.Errors.Add($"No examples generated for word '{vocabularyWord.Word}'");
                    result.ErrorCount++;
                }
                return new List<VocabularyExample>();
            }
            
            var examplesAddedCount = 0;
            var exampleIndex = 0;
            var newExamplesToAdd = new List<VocabularyExample>();
            
            foreach (var exampleData in examplesData)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(exampleData.Sentence))
                    {
                        _logger.LogWarning("Empty sentence in example data for word '{Word}'", vocabularyWord.Word);
                        exampleIndex++;
                        continue;
                    }
                    
                    // Determine difficulty level by example index
                    var difficultyLevels = request.DifficultyLevels ?? new List<DifficultyLevel> { DifficultyLevel.Easy };
                    var examplesPerLevel = request.ExampleCount;
                    var levelIndex = exampleIndex / examplesPerLevel;
                    var difficultyLevel = levelIndex < difficultyLevels.Count 
                        ? (int)difficultyLevels[levelIndex] 
                        : (int)DifficultyLevel.Easy;
                    
                    var newExample = new VocabularyExample
                    {
                        Sentence = exampleData.Sentence,
                        Phonetic = exampleData.Phonetic,
                        Translation = exampleData.Translation,
                        Grammar = exampleData.Grammar,
                        WordId = vocabularyWord.Id,
                        IsActive = true,
                        DifficultyLevel = difficultyLevel,
                        DisplayOrder = 0
                    };

                    newExamplesToAdd.Add(newExample);
                    examplesAddedCount++;
                    exampleIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing individual example for word '{Word}': {ErrorMessage}", 
                        vocabularyWord.Word, ex.Message);
                    lock (result)
                    {
                        result.Errors.Add($"Error processing example for '{vocabularyWord.Word}': {ex.Message}");
                        result.ErrorCount++;
                    }
                    exampleIndex++;
                }
            }
            
            // Update counts in thread-safe manner
            lock (result)
            {
                result.SuccessCount += examplesAddedCount;
            }

            wordStopwatch.Stop();
            _logger.LogDebug("Completed processing for '{Word}' in {ElapsedMs}ms. Added: {AddedCount} examples", 
                vocabularyWord.Word, wordStopwatch.ElapsedMilliseconds, examplesAddedCount);

            return newExamplesToAdd;
        }
        catch (Exception ex)
        {
            wordStopwatch.Stop();
            _logger.LogError(ex, "Failed to generate examples for '{Word}' after {ElapsedMs}ms. Error: {ErrorMessage}", 
                vocabularyWord.Word, wordStopwatch.ElapsedMilliseconds, ex.Message);
            
            var errorMessage = ex.InnerException != null 
                ? $"Failed to generate examples for '{vocabularyWord.Word}': {ex.Message} (Inner: {ex.InnerException.Message})"
                : $"Failed to generate examples for '{vocabularyWord.Word}': {ex.Message}";
            
            lock (result)
            {
                result.Errors.Add(errorMessage);
                result.ErrorCount++;
            }
            
            return new List<VocabularyExample>();
        }
    }

    private async Task<List<ChatGPTExampleResponse>> GetExamplesDataFromChatGPT(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        var timeoutSeconds = _configuration.GetValue<int>("ChatGPT:RequestTimeoutSeconds", 120);
        var maxRetries = _configuration.GetValue<int>("ChatGPT:MaxRetries", 3);
        
        var client = new ChatClient("gpt-4o", apiKey);
        var prompt = CreatePromptForExamples(vocabularyWord, request);
        
        _logger.LogInformation("🚀 Starting ChatGPT API call for '{Word}' with {MaxRetries} max retries and {TimeoutSeconds}s timeout", 
            vocabularyWord, maxRetries, timeoutSeconds);
        
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var attemptStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("⏳ ChatGPT API attempt {Attempt}/{MaxRetries} for '{Word}'...", attempt, maxRetries, vocabularyWord);
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(prompt)
                };
                
                var chatCompletion = await client.CompleteChatAsync(messages, cancellationToken: cts.Token);
                attemptStopwatch.Stop();
                
                _logger.LogInformation("✅ ChatGPT API SUCCESS on attempt {Attempt} for '{Word}' in {ElapsedMs}ms ({ElapsedSec}s)", 
                    attempt, vocabularyWord, attemptStopwatch.ElapsedMilliseconds, attemptStopwatch.ElapsedMilliseconds / 1000.0);
                
                var content = chatCompletion.Value.Content[0].Text;
                
                // Clean up the response to ensure valid JSON
                var jsonStart = content.IndexOf('[');
                var jsonEnd = content.LastIndexOf(']');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var examples = JsonSerializer.Deserialize<List<ChatGPTExampleResponse>>(jsonContent) ?? new List<ChatGPTExampleResponse>();
                    _logger.LogInformation("📝 Parsed {Count} examples for '{Word}'", examples.Count, vocabularyWord);
                    return examples;
                }
                
                throw new InvalidOperationException("Invalid JSON response from ChatGPT");
            }
            catch (OperationCanceledException ex)
            {
                attemptStopwatch.Stop();
                lastException = ex;
                
                _logger.LogWarning("⏰ TIMEOUT: ChatGPT API attempt {Attempt}/{MaxRetries} for '{Word}' after {ElapsedMs}ms ({ElapsedSec}s). Timeout was {TimeoutSeconds}s", 
                    attempt, maxRetries, vocabularyWord, attemptStopwatch.ElapsedMilliseconds, attemptStopwatch.ElapsedMilliseconds / 1000.0, timeoutSeconds);
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    _logger.LogInformation("🔄 Retrying ChatGPT API for '{Word}' after {DelaySeconds}s delay...", vocabularyWord, delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                attemptStopwatch.Stop();
                lastException = ex;
                
                _logger.LogWarning("❌ ChatGPT API attempt {Attempt}/{MaxRetries} FAILED for '{Word}' after {ElapsedMs}ms ({ElapsedSec}s). Error: {ErrorType} - {ErrorMessage}", 
                    attempt, maxRetries, vocabularyWord, attemptStopwatch.ElapsedMilliseconds, attemptStopwatch.ElapsedMilliseconds / 1000.0, 
                    ex.GetType().Name, ex.Message);
                
                if (attempt < maxRetries)
                {
                    // Exponential backoff: wait 2s, 4s, 8s...
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogInformation("🔄 Retrying ChatGPT API for '{Word}' after {DelaySeconds}s delay...", vocabularyWord, delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
        }
        
        _logger.LogError("💥 ChatGPT API FAILED for '{Word}' after ALL {MaxRetries} attempts. Final error: {ErrorType} - {ErrorMessage}", 
            vocabularyWord, maxRetries, lastException?.GetType().Name, lastException?.Message);
        
        throw new InvalidOperationException($"ChatGPT failed after {maxRetries} attempts: {lastException?.Message}", lastException);
    }

    private string CreatePromptForExamples(string vocabularyWord, ImportExamplesFromWordsCommand request)
    {
        var difficultyLevels = request.DifficultyLevels ?? new List<DifficultyLevel> { DifficultyLevel.Easy };
        var examplesPerLevel = request.ExampleCount;
        var totalExamples = examplesPerLevel * difficultyLevels.Count;

        return $@"Create {totalExamples} example sentences using '{vocabularyWord}' in various contexts.

Return ONLY valid JSON array:
[{{""Sentence"":""sentence with '{vocabularyWord}'"",""Phonetic"":""/fəˈnetɪk/"",""Translation"":""Vietnamese"",""Grammar"":""Vietnamese grammar explanation""}}]

Requirements:
- {examplesPerLevel} simple examples
- {examplesPerLevel} medium examples (2 clauses)
- {examplesPerLevel} complex examples (3+ clauses)
- Include IPA phonetics with stress markers
- Natural Vietnamese translations
- Grammar explanations in Vietnamese";
    }
}
