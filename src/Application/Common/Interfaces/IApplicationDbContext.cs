using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    // Vocabulary Learning System
    DbSet<VocabularyCategory> VocabularyCategories { get; }
    
    DbSet<VocabularyWord> VocabularyWords { get; }
    
    DbSet<VocabularyExample> VocabularyExamples { get; }
    
    DbSet<UserWordProgress> UserWordProgress { get; }
    
    DbSet<UserCategoryProgress> UserCategoryProgress { get; }
    
    DbSet<UserExampleProgress> UserExampleProgress { get; }
    
    DbSet<DictationResult> DictationResults { get; }
    
    DbSet<LearningSession> LearningSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
