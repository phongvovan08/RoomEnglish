using RoomEnglish.Application.Common.Mappings;
using RoomEnglish.Domain.Entities;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Queries;

public class VocabularyCategoryDto : IMapFrom<VocabularyCategory>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string IconName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int WordCount { get; init; }
    public int VocabularyCount { get; init; } // Alias for WordCount for frontend compatibility
    public DateTime Created { get; init; }
}

public class VocabularyWordDto : IMapFrom<VocabularyWord>
{
    public int Id { get; init; }
    public string Word { get; init; } = string.Empty;
    public string Phonetic { get; init; } = string.Empty;
    public string PartOfSpeech { get; init; } = string.Empty;
    public string Meaning { get; init; } = string.Empty;
    public string Definition { get; init; } = string.Empty;
    public string VietnameseMeaning { get; init; } = string.Empty;
    public string? AudioUrl { get; init; }
    public int DifficultyLevel { get; init; }
    public bool IsActive { get; init; }
    public int ViewCount { get; init; }
    public int CorrectCount { get; init; }
    public int IncorrectCount { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int ExampleCount { get; init; } // For sorting and display
    public IList<VocabularyExampleDto> Examples { get; init; } = new List<VocabularyExampleDto>();
    public UserWordProgressDto? UserProgress { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<VocabularyWord, VocabularyWordDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));
    }
}

public class VocabularyExampleDto : IMapFrom<VocabularyExample>
{
    public int Id { get; init; }
    public string Sentence { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string? Grammar { get; init; } = string.Empty;
    public string? AudioUrl { get; init; }
    public int DifficultyLevel { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int WordId { get; init; }
}

public class UserWordProgressDto : IMapFrom<UserWordProgress>
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int WordId { get; init; }
    public int StudiedTimes { get; init; }
    public int CorrectAnswers { get; init; }
    public int TotalAttempts { get; init; }
    public DateTime? FirstStudiedAt { get; init; }
    public DateTime? LastStudiedAt { get; init; }
    public bool IsMastered { get; init; }
    public int MasteryLevel { get; init; }
    public double AccuracyRate => TotalAttempts > 0 ? (double)CorrectAnswers / TotalAttempts * 100 : 0;
}

public class DictationResultDto : IMapFrom<DictationResult>
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int ExampleId { get; init; }
    public string UserInput { get; init; } = string.Empty;
    public string CorrectAnswer { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public int AccuracyPercentage { get; init; }
    public int TimeTakenSeconds { get; init; }
    public DateTime CompletedAt { get; init; }
}

public class LearningSessionDto : IMapFrom<LearningSession>
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int TotalWords { get; init; }
    public int CorrectAnswers { get; init; }
    public int DurationMinutes { get; init; }
    public string SessionType { get; init; } = string.Empty;
    public int Score { get; init; }
    public bool IsCompleted { get; init; }
    public double AccuracyRate => TotalWords > 0 ? (double)CorrectAnswers / TotalWords * 100 : 0;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<LearningSession, LearningSessionDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));
    }
}