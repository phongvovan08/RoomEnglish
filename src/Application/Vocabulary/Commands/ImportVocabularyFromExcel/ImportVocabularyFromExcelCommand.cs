using MediatR;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace RoomEnglish.Application.Vocabulary.Commands.ImportVocabularyFromExcel;

public record ImportVocabularyFromExcelCommand : IRequest<ImportVocabularyResult>
{
    public Stream ExcelStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
}

public class ImportVocabularyResult
{
    public bool Success { get; set; }
    public int ImportedWords { get; set; }
    public int ImportedExamples { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ImportVocabularyFromExcelCommandHandler : IRequestHandler<ImportVocabularyFromExcelCommand, ImportVocabularyResult>
{
    private readonly IApplicationDbContext _context;

    public ImportVocabularyFromExcelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportVocabularyResult> Handle(ImportVocabularyFromExcelCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportVocabularyResult();
        
        try
        {
            // Create ExcelPackage - handle license if needed
            using var package = new ExcelPackage(request.ExcelStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                result.Errors.Add("No worksheet found in Excel file");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1)
            {
                result.Errors.Add("Excel file contains no data rows");
                return result;
            }

            // Validate headers
            var expectedHeaders = new[] { "Word", "Phonetic", "PartOfSpeech", "Meaning", "Definition", "DifficultyLevel", "Category", "ExampleSentence", "ExampleTranslation", "ExampleGrammar" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= Math.Min(expectedHeaders.Length, worksheet.Dimension?.Columns ?? 0); col++)
            {
                actualHeaders.Add(worksheet.Cells[1, col].Value?.ToString() ?? "");
            }

            var missingHeaders = expectedHeaders.Where(h => !actualHeaders.Contains(h)).ToList();
            if (missingHeaders.Any())
            {
                result.Errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return result;
            }

            // Process data rows
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    await ProcessRow(worksheet, row, result, cancellationToken);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing row {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing Excel file: {ex.Message}");
        }

        return result;
    }

    private async Task ProcessRow(ExcelWorksheet worksheet, int row, ImportVocabularyResult result, CancellationToken cancellationToken)
    {
        var word = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
        if (string.IsNullOrEmpty(word))
        {
            result.Warnings.Add($"Row {row}: Empty word, skipping");
            return;
        }

        // Check if word already exists
        var existingWord = await _context.VocabularyWords
            .FirstOrDefaultAsync(w => w.Word.ToLower() == word.ToLower(), cancellationToken);

        VocabularyWord vocabularyWord;
        
        if (existingWord != null)
        {
            vocabularyWord = existingWord;
            result.Warnings.Add($"Row {row}: Word '{word}' already exists, updating");
        }
        else
        {
            vocabularyWord = new VocabularyWord();
            _context.VocabularyWords.Add(vocabularyWord);
            result.ImportedWords++;
        }

        // Update word properties
        vocabularyWord.Word = word;
        vocabularyWord.Phonetic = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
        vocabularyWord.PartOfSpeech = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
        vocabularyWord.Meaning = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
        vocabularyWord.Definition = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
        
        if (int.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out var difficulty))
        {
            vocabularyWord.DifficultyLevel = Math.Max(1, Math.Min(3, difficulty));
        }
        else
        {
            vocabularyWord.DifficultyLevel = 1;
        }

        // Handle category - find or create category
        var categoryName = worksheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "General";
        var category = await _context.VocabularyCategories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower(), cancellationToken);
        
        if (category == null)
        {
            category = new VocabularyCategory
            {
                Name = categoryName,
                Description = $"Category for {categoryName} words",
                Color = "Blue",
                IconName = "mdi:book",
                IsActive = true,
                DisplayOrder = 0
            };
            _context.VocabularyCategories.Add(category);
        }
        
        vocabularyWord.Category = category;
        vocabularyWord.CategoryId = category.Id;
        vocabularyWord.IsActive = true;

        // Process example if provided
        var exampleSentence = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
        if (!string.IsNullOrEmpty(exampleSentence))
        {
            var exampleTranslation = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
            var exampleGrammar = worksheet.Cells[row, 10].Value?.ToString()?.Trim();

            var example = new VocabularyExample
            {
                Word = vocabularyWord,
                Sentence = exampleSentence,
                Translation = exampleTranslation ?? "",
                Grammar = exampleGrammar,
                DifficultyLevel = vocabularyWord.DifficultyLevel,
                DisplayOrder = 1,
                IsActive = true
            };

            _context.VocabularyExamples.Add(example);
            result.ImportedExamples++;
        }
    }
}