using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Exceptions;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Application.Vocabulary.Commands.UpdateUserProgress;
using RoomEnglish.Domain.Entities;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Commands.SubmitDictation;

public record SubmitDictationCommand : IRequest<DictationResultDto>
{
    public int ExampleId { get; init; }
    public string UserInput { get; init; } = string.Empty;
    public int TimeTakenSeconds { get; init; }
}

public class SubmitDictationCommandHandler : IRequestHandler<SubmitDictationCommand, DictationResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public SubmitDictationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IMapper mapper,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
        _sender = sender;
    }

    public async Task<DictationResultDto> Handle(SubmitDictationCommand request, CancellationToken cancellationToken)
    {
        var example = await _context.VocabularyExamples
            .Include(x => x.Word)
            .FirstOrDefaultAsync(x => x.Id == request.ExampleId, cancellationToken);

        if (example == null)
        {
            throw new NotFoundException(nameof(VocabularyExample), request.ExampleId.ToString());
        }

        var userId = _currentUser.Id!;
        var correctAnswer = example.Sentence.Trim();
        var userInput = request.UserInput.Trim();
        
        // Calculate accuracy using Levenshtein distance
        var accuracy = CalculateAccuracy(correctAnswer, userInput);
        var isCorrect = accuracy >= 80; // 80% threshold for correct

        // Create dictation result
        var result = new DictationResult
        {
            UserId = userId,
            ExampleId = request.ExampleId,
            UserInput = userInput,
            CorrectAnswer = correctAnswer,
            IsCorrect = isCorrect,
            AccuracyPercentage = accuracy,
            TimeTakenSeconds = request.TimeTakenSeconds,
            CompletedAt = DateTime.UtcNow
        };

        _context.DictationResults.Add(result);

        // Update user word progress
        await UpdateUserWordProgress(example.WordId, userId, isCorrect, cancellationToken);

        // Update example progress
        await _sender.Send(new UpdateExampleProgressCommand
        {
            ExampleId = request.ExampleId,
            AccuracyPercentage = accuracy,
            UserId = userId
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DictationResultDto>(result);
    }

    private async Task UpdateUserWordProgress(int wordId, string userId, bool isCorrect, CancellationToken cancellationToken)
    {
        var progress = await _context.UserWordProgress
            .FirstOrDefaultAsync(x => x.WordId == wordId && x.UserId == userId, cancellationToken);

        if (progress == null)
        {
            progress = new UserWordProgress
            {
                WordId = wordId,
                UserId = userId,
                FirstStudiedAt = DateTime.UtcNow
            };
            _context.UserWordProgress.Add(progress);
        }

        progress.TotalAttempts++;
        progress.StudiedTimes++;
        progress.LastStudiedAt = DateTime.UtcNow;

        if (isCorrect)
        {
            progress.CorrectAnswers++;
        }

        // Update mastery level
        var accuracyRate = (double)progress.CorrectAnswers / progress.TotalAttempts;
        progress.MasteryLevel = (int)(accuracyRate * 100);
        progress.IsMastered = accuracyRate >= 0.8 && progress.TotalAttempts >= 3;
    }

    private static int CalculateAccuracy(string correct, string input)
    {
        if (string.IsNullOrEmpty(correct) && string.IsNullOrEmpty(input))
            return 100;
        
        if (string.IsNullOrEmpty(correct) || string.IsNullOrEmpty(input))
            return 0;

        var distance = LevenshteinDistance(correct.ToLower(), input.ToLower());
        var maxLength = Math.Max(correct.Length, input.Length);
        var accuracy = (int)(((double)(maxLength - distance) / maxLength) * 100);
        
        return Math.Max(0, accuracy);
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b.Length;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var matrix = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= b.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                var cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[a.Length, b.Length];
    }
}