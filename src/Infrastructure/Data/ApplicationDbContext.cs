using System.Reflection;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;
using RoomEnglish.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RoomEnglish.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    // Vocabulary Learning System
    public DbSet<VocabularyCategory> VocabularyCategories => Set<VocabularyCategory>();
    
    public DbSet<VocabularyWord> VocabularyWords => Set<VocabularyWord>();
    
    public DbSet<VocabularyExample> VocabularyExamples => Set<VocabularyExample>();
    
    public DbSet<UserWordProgress> UserWordProgress => Set<UserWordProgress>();
    
    public DbSet<UserCategoryProgress> UserCategoryProgress => Set<UserCategoryProgress>();
    
    public DbSet<UserExampleProgress> UserExampleProgress => Set<UserExampleProgress>();
    
    public DbSet<DictationResult> DictationResults => Set<DictationResult>();
    
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
