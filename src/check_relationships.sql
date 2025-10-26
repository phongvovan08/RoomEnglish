-- ============================================
-- Kiểm Tra Quan Hệ Khóa Chính - Khóa Ngoại
-- VocabularyCategories → VocabularyWords → VocabularyExamples
-- ============================================

USE RoomEnglishDb;
GO

-- 1. Kiểm tra Foreign Key Constraints
PRINT '========================================';
PRINT '1. FOREIGN KEY CONSTRAINTS';
PRINT '========================================';

SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn,
    fk.delete_referential_action_desc AS DeleteAction,
    fk.update_referential_action_desc AS UpdateAction
FROM 
    sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc 
        ON fk.object_id = fc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) IN ('VocabularyWords', 'VocabularyExamples')
ORDER BY 
    TableName, ForeignKeyName;

PRINT '';
PRINT '========================================';
PRINT '2. PRIMARY KEYS';
PRINT '========================================';

-- 2. Kiểm tra Primary Keys
SELECT 
    t.name AS TableName,
    i.name AS PrimaryKeyName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
    ic.index_column_id AS ColumnPosition
FROM 
    sys.tables AS t
    INNER JOIN sys.indexes AS i 
        ON t.object_id = i.object_id
    INNER JOIN sys.index_columns AS ic 
        ON i.object_id = ic.object_id 
        AND i.index_id = ic.index_id
WHERE 
    i.is_primary_key = 1
    AND t.name IN ('VocabularyCategories', 'VocabularyWords', 'VocabularyExamples')
ORDER BY 
    t.name, ic.index_column_id;

PRINT '';
PRINT '========================================';
PRINT '3. DATA INTEGRITY CHECK';
PRINT '========================================';

-- 3. Kiểm tra Data Integrity
PRINT 'Categories with Words:';
SELECT 
    c.Id,
    c.Name AS CategoryName,
    COUNT(w.Id) AS WordCount
FROM 
    VocabularyCategories c
    LEFT JOIN VocabularyWords w ON c.Id = w.CategoryId
GROUP BY 
    c.Id, c.Name
ORDER BY 
    c.Id;

PRINT '';
PRINT 'Words with Examples:';
SELECT 
    w.Id,
    w.Word,
    c.Name AS CategoryName,
    COUNT(e.Id) AS ExampleCount
FROM 
    VocabularyWords w
    INNER JOIN VocabularyCategories c ON w.CategoryId = c.Id
    LEFT JOIN VocabularyExamples e ON w.Id = e.WordId
GROUP BY 
    w.Id, w.Word, c.Name
ORDER BY 
    w.Id;

PRINT '';
PRINT '========================================';
PRINT '4. ORPHANED RECORDS CHECK';
PRINT '========================================';

-- 4. Kiểm tra Orphaned Records (không nên có)
PRINT 'Words without Category (Orphaned):';
SELECT 
    Id, 
    Word, 
    CategoryId
FROM 
    VocabularyWords
WHERE 
    CategoryId NOT IN (SELECT Id FROM VocabularyCategories);

PRINT '';
PRINT 'Examples without Word (Orphaned):';
SELECT 
    Id, 
    Sentence, 
    WordId
FROM 
    VocabularyExamples
WHERE 
    WordId NOT IN (SELECT Id FROM VocabularyWords);

PRINT '';
PRINT '========================================';
PRINT '5. RELATIONSHIP STRUCTURE';
PRINT '========================================';

-- 5. Hiển thị cấu trúc quan hệ đầy đủ
SELECT 
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    w.Id AS WordId,
    w.Word,
    e.Id AS ExampleId,
    LEFT(e.Sentence, 50) AS ExampleSentence
FROM 
    VocabularyCategories c
    LEFT JOIN VocabularyWords w ON c.Id = w.CategoryId
    LEFT JOIN VocabularyExamples e ON w.Id = e.WordId
ORDER BY 
    c.Id, w.Id, e.Id;

PRINT '';
PRINT '========================================';
PRINT '6. CASCADE DELETE TEST (DRY RUN)';
PRINT '========================================';

-- 6. Mô phỏng cascade delete (không thực thi thật)
PRINT 'If we delete Category Id = 1, these records will be affected:';
PRINT '';
PRINT 'Words that will be deleted:';
SELECT Id, Word FROM VocabularyWords WHERE CategoryId = 1;

PRINT '';
PRINT 'Examples that will be deleted (via cascade):';
SELECT 
    e.Id, 
    e.Sentence, 
    e.WordId
FROM 
    VocabularyExamples e
    INNER JOIN VocabularyWords w ON e.WordId = w.Id
WHERE 
    w.CategoryId = 1;

PRINT '';
PRINT '========================================';
PRINT '7. INDEXES ON FOREIGN KEYS';
PRINT '========================================';

-- 7. Kiểm tra indexes trên foreign keys (performance)
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM 
    sys.tables AS t
    INNER JOIN sys.indexes AS i 
        ON t.object_id = i.object_id
    INNER JOIN sys.index_columns AS ic 
        ON i.object_id = ic.object_id 
        AND i.index_id = ic.index_id
WHERE 
    t.name IN ('VocabularyWords', 'VocabularyExamples')
    AND COL_NAME(ic.object_id, ic.column_id) IN ('CategoryId', 'WordId')
ORDER BY 
    t.name, i.name;

PRINT '';
PRINT '========================================';
PRINT 'SUMMARY';
PRINT '========================================';
PRINT 'Check completed successfully!';
PRINT 'Review the results above to ensure:';
PRINT '1. Foreign keys are properly defined';
PRINT '2. Primary keys exist on all tables';
PRINT '3. No orphaned records';
PRINT '4. Cascade delete is configured correctly';
PRINT '5. Indexes exist on foreign key columns for performance';
PRINT '========================================';
