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
    public int VocabularyId { get; init; }
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
            TotalProcessed = 1 // Processing one vocabulary word
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

            // Process the single vocabulary word to generate examples
            await ProcessVocabularyWord(vocabularyWord, result, request, cancellationToken);

            result.Success = result.ErrorCount == 0;
            result.Message = result.Success 
                ? $"Successfully generated {result.SuccessCount} examples for '{vocabularyWord.Word}'"
                : $"Generated examples with {result.ErrorCount} errors";

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

    private async Task ProcessVocabularyWord(VocabularyWord vocabularyWord, ImportExamplesWordsResult result, ImportExamplesFromWordsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var examplesData = await GetExamplesDataFromChatGPT(vocabularyWord.Word, request);
            
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
                            DifficultyLevel = (int)(request.DifficultyLevel ?? Commands.ImportExamplesFromWords.DifficultyLevel.Easy),
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

        var client = new ChatClient("gpt-4o", apiKey);
        
        var prompt = CreatePromptForExamples(vocabularyWord, request);

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
