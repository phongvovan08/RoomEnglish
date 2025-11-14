using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Diagnostics;

namespace RoomEnglish.Application.Vocabulary.Commands.TranslateVietnameseMeaning;

public record TranslateVietnameseMeaningCommand : IRequest<TranslateVietnameseMeaningResult>
{
    public int? WordId { get; init; }
    public bool TranslateAll { get; init; } = false;
    public int BatchSize { get; init; } = 10;
}

public class TranslateVietnameseMeaningResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TranslateVietnameseMeaningCommandHandler : IRequestHandler<TranslateVietnameseMeaningCommand, TranslateVietnameseMeaningResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslateVietnameseMeaningCommandHandler> _logger;

    public TranslateVietnameseMeaningCommandHandler(
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<TranslateVietnameseMeaningCommandHandler> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TranslateVietnameseMeaningResult> Handle(
        TranslateVietnameseMeaningCommand request,
        CancellationToken cancellationToken)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var result = new TranslateVietnameseMeaningResult();

        try
        {
            List<Domain.Entities.VocabularyWord> wordsToTranslate;

            // Get words that need translation
            if (request.WordId.HasValue)
            {
                // Translate specific word
                var word = await _context.VocabularyWords
                    .Where(w => w.Id == request.WordId.Value && w.IsActive)
                    .FirstOrDefaultAsync(cancellationToken);

                if (word == null)
                {
                    result.Errors.Add($"Word with ID {request.WordId.Value} not found");
                    result.Success = false;
                    result.Message = "Word not found";
                    return result;
                }

                wordsToTranslate = new List<Domain.Entities.VocabularyWord> { word };
            }
            else if (request.TranslateAll)
            {
                // Get all words with null VietnameseMeaning
                wordsToTranslate = await _context.VocabularyWords
                    .Where(w => w.IsActive && 
                           (w.VietnameseMeaning == null || w.VietnameseMeaning == string.Empty) &&
                           w.Definition != null && w.Definition != string.Empty)
                    .Take(request.BatchSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} words needing Vietnamese translation", wordsToTranslate.Count);
            }
            else
            {
                result.Errors.Add("Either WordId or TranslateAll must be specified");
                result.Success = false;
                result.Message = "Invalid request parameters";
                return result;
            }

            if (!wordsToTranslate.Any())
            {
                result.Success = true;
                result.Message = "No words need translation";
                return result;
            }

            result.TotalProcessed = wordsToTranslate.Count;

            // Process words in batches to optimize ChatGPT calls
            var batchStopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation("🚀 Translating {Count} words in one ChatGPT call...", wordsToTranslate.Count);
            
            try
            {
                var translations = await TranslateMultipleToVietnamese(wordsToTranslate);
                
                // Apply translations to words
                for (int i = 0; i < wordsToTranslate.Count; i++)
                {
                    var word = wordsToTranslate[i];
                    if (i < translations.Count && !string.IsNullOrEmpty(translations[i]))
                    {
                        word.VietnameseMeaning = translations[i];
                        result.SuccessCount++;
                        _logger.LogDebug("✅ Translated '{Word}': {Translation}", word.Word, translations[i]);
                    }
                    else
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"No translation received for '{word.Word}'");
                        _logger.LogWarning("⚠️ Missing translation for '{Word}'", word.Word);
                    }
                }
                
                batchStopwatch.Stop();
                _logger.LogInformation("✅ Batch translation completed in {ElapsedMs}ms ({ElapsedSec}s)", 
                    batchStopwatch.ElapsedMilliseconds, batchStopwatch.ElapsedMilliseconds / 1000.0);
            }
            catch (Exception ex)
            {
                batchStopwatch.Stop();
                result.ErrorCount += wordsToTranslate.Count;
                result.Errors.Add($"Batch translation failed: {ex.Message}");
                
                _logger.LogError(ex, "❌ Batch translation failed after {ElapsedMs}ms", 
                    batchStopwatch.ElapsedMilliseconds);
            }

            // Save all changes
            var saveStopwatch = Stopwatch.StartNew();
            await _context.SaveChangesAsync(cancellationToken);
            saveStopwatch.Stop();
            totalStopwatch.Stop();

            result.Success = result.SuccessCount > 0;
            result.Message = result.Success 
                ? $"Successfully translated {result.SuccessCount} words. {result.ErrorCount} errors occurred."
                : $"Translation failed: {result.ErrorCount} errors";

            _logger.LogInformation("Translation completed in {TotalMs}ms. Translated: {SuccessCount}, Errors: {ErrorCount}", 
                totalStopwatch.ElapsedMilliseconds, result.SuccessCount, result.ErrorCount);

            return result;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.ErrorCount++;
            result.Success = false;
            result.Message = "Translation process failed";
            
            _logger.LogError(ex, "Unexpected error during translation after {TotalMs}ms", 
                totalStopwatch.ElapsedMilliseconds);
            
            return result;
        }
    }

    private async Task<List<string>> TranslateMultipleToVietnamese(List<Domain.Entities.VocabularyWord> words)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        var timeoutSeconds = _configuration.GetValue<int>("ChatGPT:RequestTimeoutSeconds", 180);
        var maxRetries = _configuration.GetValue<int>("ChatGPT:MaxRetries", 2);
        
        var client = new ChatClient("gpt-4o", apiKey);
        
        // Build numbered list of words with definitions
        var wordsList = new System.Text.StringBuilder();
        for (int i = 0; i < words.Count; i++)
        {
            wordsList.AppendLine($"{i + 1}. Word: {words[i].Word}");
            wordsList.AppendLine($"   Definition: {words[i].Definition}");
            wordsList.AppendLine();
        }
        
        var prompt = $@"Translate the following English definitions to Vietnamese. Return ONLY a JSON array of translations.

{wordsList}

Return format:
[""Vietnamese translation 1"",""Vietnamese translation 2"",...]

Requirements:
- Natural, clear Vietnamese translations
- Accurate and concise
- Proper Vietnamese grammar
- Return ONLY the JSON array, no extra text";

        Exception? lastException = null;
        
        _logger.LogInformation("📤 Sending {Count} definitions to ChatGPT for translation...", words.Count);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var attemptStopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("⏳ Translation attempt {Attempt}/{MaxRetries}...", attempt, maxRetries);
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(prompt)
                };
                
                var chatCompletion = await client.CompleteChatAsync(messages, cancellationToken: cts.Token);
                attemptStopwatch.Stop();
                
                var content = chatCompletion.Value.Content[0].Text;
                
                _logger.LogInformation("✅ ChatGPT responded in {ElapsedMs}ms ({ElapsedSec}s)", 
                    attemptStopwatch.ElapsedMilliseconds, attemptStopwatch.ElapsedMilliseconds / 1000.0);
                
                // Parse JSON array
                var jsonStart = content.IndexOf('[');
                var jsonEnd = content.LastIndexOf(']');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var translations = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonContent) 
                        ?? new List<string>();
                    
                    _logger.LogInformation("📝 Parsed {Count} translations", translations.Count);
                    return translations;
                }
                
                throw new InvalidOperationException("Invalid JSON response from ChatGPT");
            }
            catch (Exception ex)
            {
                attemptStopwatch.Stop();
                lastException = ex;
                
                _logger.LogWarning("⚠️ Translation attempt {Attempt}/{MaxRetries} failed after {ElapsedMs}ms: {Error}", 
                    attempt, maxRetries, attemptStopwatch.ElapsedMilliseconds, ex.Message);
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogInformation("🔄 Retrying after {DelaySeconds}s delay...", delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new InvalidOperationException($"ChatGPT translation failed after {maxRetries} attempts: {lastException?.Message}", lastException);
    }
}
