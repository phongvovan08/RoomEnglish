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

            // Process each word
            foreach (var word in wordsToTranslate)
            {
                var wordStopwatch = Stopwatch.StartNew();
                
                try
                {
                    _logger.LogInformation("Translating Vietnamese meaning for word: {Word} (ID: {Id})", 
                        word.Word, word.Id);

                    var translation = await TranslateToVietnamese(word.Definition, word.Word);
                    
                    word.VietnameseMeaning = translation;
                    result.SuccessCount++;
                    
                    wordStopwatch.Stop();
                    _logger.LogInformation("Successfully translated '{Word}' in {ElapsedMs}ms: {Translation}", 
                        word.Word, wordStopwatch.ElapsedMilliseconds, translation);
                }
                catch (Exception ex)
                {
                    wordStopwatch.Stop();
                    result.ErrorCount++;
                    var errorMsg = $"Failed to translate '{word.Word}': {ex.Message}";
                    result.Errors.Add(errorMsg);
                    
                    _logger.LogError(ex, "Translation failed for '{Word}' after {ElapsedMs}ms", 
                        word.Word, wordStopwatch.ElapsedMilliseconds);
                }
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

    private async Task<string> TranslateToVietnamese(string definition, string word)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured");
        }

        var timeoutSeconds = _configuration.GetValue<int>("ChatGPT:RequestTimeoutSeconds", 30);
        var maxRetries = _configuration.GetValue<int>("ChatGPT:MaxRetries", 3);
        
        var client = new ChatClient("gpt-4o", apiKey);
        
        var prompt = $@"Translate the following English definition to Vietnamese. 
The definition is for the English word: ""{word}""

Definition: {definition}

Requirements:
- Provide a natural, clear Vietnamese translation
- Keep the meaning accurate and concise
- Use proper Vietnamese grammar
- Return ONLY the Vietnamese translation, no extra text or explanation

Vietnamese translation:";

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
                var translation = chatCompletion.Value.Content[0].Text.Trim();
                
                return translation;
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                _logger.LogWarning("ChatGPT translation attempt {Attempt}/{MaxRetries} failed for '{Word}': {Error}", 
                    attempt, maxRetries, word, ex.Message);
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new InvalidOperationException($"ChatGPT translation failed after {maxRetries} attempts: {lastException?.Message}");
    }
}
