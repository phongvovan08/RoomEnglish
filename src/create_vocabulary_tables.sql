-- Add Vocabulary Learning System Tables
-- This script adds only the vocabulary-related tables without affecting existing Identity tables

-- Create VocabularyCategories table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VocabularyCategories' AND xtype='U')
CREATE TABLE [dbo].[VocabularyCategories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_VocabularyCategories] PRIMARY KEY ([Id])
);
GO

-- Create VocabularyWords table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VocabularyWords' AND xtype='U')
CREATE TABLE [dbo].[VocabularyWords] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Word] nvarchar(100) NOT NULL,
    [Definition] nvarchar(max) NOT NULL,
    [Pronunciation] nvarchar(200) NULL,
    [AudioUrl] nvarchar(500) NULL,
    [CategoryId] int NOT NULL,
    [DifficultyLevel] int NOT NULL DEFAULT 1,
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_VocabularyWords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VocabularyWords_VocabularyCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [VocabularyCategories] ([Id]) ON DELETE CASCADE
);
GO

-- Create VocabularyExamples table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VocabularyExamples' AND xtype='U')
CREATE TABLE [dbo].[VocabularyExamples] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Sentence] nvarchar(max) NOT NULL,
    [Translation] nvarchar(max) NULL,
    [AudioUrl] nvarchar(500) NULL,
    [WordId] int NOT NULL,
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_VocabularyExamples] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VocabularyExamples_VocabularyWords_WordId] FOREIGN KEY ([WordId]) REFERENCES [VocabularyWords] ([Id]) ON DELETE CASCADE
);
GO

-- Create UserWordProgress table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserWordProgress' AND xtype='U')
CREATE TABLE [dbo].[UserWordProgress] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [WordId] int NOT NULL,
    [CorrectCount] int NOT NULL DEFAULT 0,
    [IncorrectCount] int NOT NULL DEFAULT 0,
    [LastStudied] datetime2 NULL,
    [MasteryLevel] int NOT NULL DEFAULT 0,
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_UserWordProgress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserWordProgress_VocabularyWords_WordId] FOREIGN KEY ([WordId]) REFERENCES [VocabularyWords] ([Id]) ON DELETE CASCADE
);
GO

-- Create DictationResults table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DictationResults' AND xtype='U')
CREATE TABLE [dbo].[DictationResults] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [WordId] int NOT NULL,
    [UserInput] nvarchar(max) NOT NULL,
    [CorrectAnswer] nvarchar(max) NOT NULL,
    [AccuracyPercentage] decimal(5,2) NOT NULL,
    [IsCorrect] bit NOT NULL,
    [CompletedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_DictationResults] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DictationResults_VocabularyWords_WordId] FOREIGN KEY ([WordId]) REFERENCES [VocabularyWords] ([Id]) ON DELETE CASCADE
);
GO

-- Create LearningSessions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LearningSessions' AND xtype='U')
CREATE TABLE [dbo].[LearningSessions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [CategoryId] int NOT NULL,
    [StartedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [CompletedAt] datetime2 NULL,
    [TotalWords] int NOT NULL,
    [CorrectAnswers] int NOT NULL DEFAULT 0,
    [Score] decimal(5,2) NOT NULL DEFAULT 0,
    [Created] datetime2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LearningSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LearningSessions_VocabularyCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [VocabularyCategories] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_VocabularyWords_CategoryId] ON [dbo].[VocabularyWords] ([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_VocabularyExamples_WordId] ON [dbo].[VocabularyExamples] ([WordId]);
CREATE NONCLUSTERED INDEX [IX_UserWordProgress_WordId] ON [dbo].[UserWordProgress] ([WordId]);
CREATE NONCLUSTERED INDEX [IX_UserWordProgress_UserId] ON [dbo].[UserWordProgress] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_DictationResults_WordId] ON [dbo].[DictationResults] ([WordId]);
CREATE NONCLUSTERED INDEX [IX_DictationResults_UserId] ON [dbo].[DictationResults] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_LearningSessions_CategoryId] ON [dbo].[LearningSessions] ([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_LearningSessions_UserId] ON [dbo].[LearningSessions] ([UserId]);
GO

-- Insert sample vocabulary data
INSERT INTO [VocabularyCategories] ([Name], [Description], [Created])
VALUES 
('Basic Vocabulary', 'Từ vựng cơ bản hằng ngày', GETDATE()),
('Business English', 'Từ vựng tiếng Anh thương mại', GETDATE()),
('Travel & Tourism', 'Từ vựng về du lịch', GETDATE()),
('Technology', 'Từ vựng công nghệ thông tin', GETDATE()),
('Food & Dining', 'Từ vựng về ẩm thực', GETDATE());
GO

-- Insert sample words for Basic Vocabulary category
DECLARE @BasicCategoryId INT = (SELECT TOP 1 Id FROM VocabularyCategories WHERE Name = 'Basic Vocabulary');

INSERT INTO [VocabularyWords] ([Word], [Definition], [Pronunciation], [CategoryId], [DifficultyLevel], [Created])
VALUES 
('Hello', 'A greeting used when meeting someone', '/həˈloʊ/', @BasicCategoryId, 1, GETDATE()),
('Goodbye', 'A farewell expression', '/ɡʊdˈbaɪ/', @BasicCategoryId, 1, GETDATE()),
('Please', 'Used to make a polite request', '/pliːz/', @BasicCategoryId, 1, GETDATE()),
('Thank you', 'An expression of gratitude', '/θæŋk juː/', @BasicCategoryId, 1, GETDATE()),
('Excuse me', 'Used to get someone''s attention politely', '/ɪkˈskjuːz miː/', @BasicCategoryId, 2, GETDATE());
GO

-- Insert sample words for Technology category
DECLARE @TechCategoryId INT = (SELECT TOP 1 Id FROM VocabularyCategories WHERE Name = 'Technology');

INSERT INTO [VocabularyWords] ([Word], [Definition], [Pronunciation], [CategoryId], [DifficultyLevel], [Created])
VALUES 
('Computer', 'An electronic device for processing data', '/kəmˈpjuːtər/', @TechCategoryId, 2, GETDATE()),
('Software', 'Programs used to operate computers', '/ˈsɔːftwer/', @TechCategoryId, 3, GETDATE()),
('Internet', 'A global network connecting computers', '/ˈɪntərnet/', @TechCategoryId, 2, GETDATE()),
('Website', 'A location on the World Wide Web', '/ˈwebsaɪt/', @TechCategoryId, 2, GETDATE()),
('Programming', 'The process of creating computer programs', '/ˈproʊɡræmɪŋ/', @TechCategoryId, 4, GETDATE());
GO

-- Insert sample examples for some words
DECLARE @HelloWordId INT = (SELECT TOP 1 Id FROM VocabularyWords WHERE Word = 'Hello');
DECLARE @ComputerWordId INT = (SELECT TOP 1 Id FROM VocabularyWords WHERE Word = 'Computer');

INSERT INTO [VocabularyExamples] ([Sentence], [Translation], [WordId], [Created])
VALUES 
('Hello, how are you today?', 'Xin chào, hôm nay bạn khỏe không?', @HelloWordId, GETDATE()),
('She said hello to everyone at the party.', 'Cô ấy chào hỏi mọi người tại bữa tiệc.', @HelloWordId, GETDATE()),
('I use my computer every day for work.', 'Tôi sử dụng máy tính hằng ngày để làm việc.', @ComputerWordId, GETDATE()),
('This computer is very fast and efficient.', 'Chiếc máy tính này rất nhanh và hiệu quả.', @ComputerWordId, GETDATE());
GO

PRINT 'Vocabulary Learning System tables created successfully!';