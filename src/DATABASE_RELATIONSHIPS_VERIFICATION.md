# Database Relationships - VocabularyCategories → Words → Examples

## 📋 Overview

Kiểm tra quan hệ khóa chính - khóa ngoại giữa 3 bảng chính trong hệ thống vocabulary learning.

## 🏗️ Database Schema

### 1. VocabularyCategories (Bảng Cha Level 1)

**Primary Key:**
- `Id` (int, IDENTITY)

**Columns:**
- `Name` (nvarchar(200), UNIQUE, NOT NULL)
- `Description` (nvarchar(1000))
- `Color` (nvarchar(50), DEFAULT 'Blue')
- `IconName` (nvarchar(100), DEFAULT 'mdi:book')
- `IsActive` (bit, DEFAULT 1)
- `DisplayOrder` (int, DEFAULT 0)
- Audit fields: `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`

**Relationships:**
- **One-to-Many** với `VocabularyWords`

---

### 2. VocabularyWords (Bảng Con Level 2)

**Primary Key:**
- `Id` (int, IDENTITY)

**Foreign Keys:**
- `CategoryId` → `VocabularyCategories.Id`
  - **Delete Behavior:** `CASCADE`
  - **Update Behavior:** `NO ACTION`

**Columns:**
- `Word` (nvarchar(200), NOT NULL)
- `Phonetic` (nvarchar(300))
- `PartOfSpeech` (nvarchar(50))
- `Meaning` (nvarchar(1000), NOT NULL)
- `Definition` (nvarchar(2000))
- `VietnameseMeaning` (nvarchar(2000))
- `AudioUrl` (nvarchar(500), nullable)
- `DifficultyLevel` (int, DEFAULT 1)
- `IsActive` (bit, DEFAULT 1)
- `ViewCount`, `CorrectCount`, `IncorrectCount` (int)
- Audit fields

**Indexes:**
- `UNIQUE INDEX` on `(Word, CategoryId)` - Tránh duplicate trong cùng category
- `INDEX` on `CategoryId` - Performance cho foreign key

**Relationships:**
- **Many-to-One** với `VocabularyCategories`
- **One-to-Many** với `VocabularyExamples`

---

### 3. VocabularyExamples (Bảng Con Level 3)

**Primary Key:**
- `Id` (int, IDENTITY)

**Foreign Keys:**
- `WordId` → `VocabularyWords.Id`
  - **Delete Behavior:** `CASCADE`
  - **Update Behavior:** `NO ACTION`

**Columns:**
- `Sentence` (nvarchar(1000), NOT NULL)
- `Translation` (nvarchar(1000), NOT NULL)
- `Grammar` (nvarchar(500), nullable)
- `AudioUrl` (nvarchar(500), nullable)
- `DifficultyLevel` (int, DEFAULT 1)
- `IsActive` (bit, DEFAULT 1)
- `DisplayOrder` (int, DEFAULT 0)
- Audit fields

**Indexes:**
- `INDEX` on `WordId` - Performance cho foreign key

**Relationships:**
- **Many-to-One** với `VocabularyWords`

---

## 🔗 Relationship Diagram

```
VocabularyCategories (1)
    ↓ (CategoryId)
    ↓ CASCADE DELETE
VocabularyWords (N)
    ↓ (WordId)
    ↓ CASCADE DELETE
VocabularyExamples (N)
```

## ✅ Entity Framework Configuration

### VocabularyCategoryConfiguration.cs

```csharp
public class VocabularyCategoryConfiguration : IEntityTypeConfiguration<VocabularyCategory>
{
    public void Configure(EntityTypeBuilder<VocabularyCategory> builder)
    {
        // Primary Key: Id (auto-configured by convention)
        
        // Properties
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        // Unique constraint
        builder.HasIndex(x => x.Name)
            .IsUnique();
            
        // No foreign keys - this is the parent table
    }
}
```

### VocabularyWordConfiguration.cs

```csharp
public class VocabularyWordConfiguration : IEntityTypeConfiguration<VocabularyWord>
{
    public void Configure(EntityTypeBuilder<VocabularyWord> builder)
    {
        // Primary Key: Id (auto-configured)
        
        // Foreign Key Relationship
        builder.HasOne(d => d.Category)
            .WithMany(p => p.Words)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);  // ✅ CASCADE DELETE
            
        // Composite Unique Index
        builder.HasIndex(x => new { x.Word, x.CategoryId })
            .IsUnique();
    }
}
```

### VocabularyExampleConfiguration.cs

```csharp
public class VocabularyExampleConfiguration : IEntityTypeConfiguration<VocabularyExample>
{
    public void Configure(EntityTypeBuilder<VocabularyExample> builder)
    {
        // Primary Key: Id (auto-configured)
        
        // Foreign Key Relationship
        builder.HasOne(d => d.Word)
            .WithMany(p => p.Examples)
            .HasForeignKey(d => d.WordId)
            .OnDelete(DeleteBehavior.Cascade);  // ✅ CASCADE DELETE
    }
}
```

## 🔍 Verification SQL Script

Chạy script `check_relationships.sql` để verify:

```bash
# Location
src/check_relationships.sql

# Run in SQL Server Management Studio or Azure Data Studio
```

### Script kiểm tra:

1. **Foreign Key Constraints** - Danh sách tất cả FK
2. **Primary Keys** - Verify PK tồn tại
3. **Data Integrity** - Đếm records theo relationship
4. **Orphaned Records** - Tìm records không có parent (không nên có)
5. **Relationship Structure** - View full hierarchy
6. **Cascade Delete Test** - Dry run để xem impact
7. **Indexes on FKs** - Performance check

## 📊 Expected Results

### Foreign Keys

| Table | FK Name | Column | Referenced Table | Referenced Column | Delete Action |
|-------|---------|--------|------------------|-------------------|---------------|
| VocabularyWords | FK_VocabularyWords_VocabularyCategories_CategoryId | CategoryId | VocabularyCategories | Id | CASCADE |
| VocabularyExamples | FK_VocabularyExamples_VocabularyWords_WordId | WordId | VocabularyWords | Id | CASCADE |

### Data Hierarchy Example

```
Category: Basic Vocabulary (Id=1)
├─ Word: "hello" (Id=1)
│  └─ Example: "Hello, how are you?" (Id=1)
├─ Word: "beautiful" (Id=2)
│  ├─ Example: "She is beautiful" (Id=2)
│  └─ Example: "The view is beautiful" (Id=3)
└─ Word: "goodbye" (Id=3)
   └─ Example: "Goodbye, see you later" (Id=4)
```

## ⚠️ Cascade Delete Behavior

### Deleting a Category

```sql
DELETE FROM VocabularyCategories WHERE Id = 1;
```

**Kết quả:**
1. ❌ Xóa Category (Id=1)
2. ❌ **CASCADE**: Xóa tất cả Words thuộc Category đó
3. ❌ **CASCADE**: Xóa tất cả Examples thuộc các Words đã xóa

### Deleting a Word

```sql
DELETE FROM VocabularyWords WHERE Id = 2;
```

**Kết quả:**
1. ❌ Xóa Word (Id=2)
2. ❌ **CASCADE**: Xóa tất cả Examples thuộc Word đó

### Deleting an Example

```sql
DELETE FROM VocabularyExamples WHERE Id = 5;
```

**Kết quả:**
1. ❌ Chỉ xóa Example (Id=5)
2. ✅ Word và Category vẫn giữ nguyên

## 🛡️ Data Integrity Protection

### Preventing Invalid Data

**1. Foreign Key Constraints:**
- ❌ Không thể insert `VocabularyWords` với `CategoryId` không tồn tại
- ❌ Không thể insert `VocabularyExamples` với `WordId` không tồn tại

**2. Unique Constraints:**
- ❌ Không thể có 2 categories cùng tên
- ❌ Không thể có 2 words giống nhau trong cùng 1 category

**3. Required Fields:**
- ❌ Category must have `Name`
- ❌ Word must have `Word` and `Meaning`
- ❌ Example must have `Sentence` and `Translation`

## 🔧 Common Queries

### Get Category with Words and Examples

```sql
SELECT 
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    w.Id AS WordId,
    w.Word,
    COUNT(e.Id) AS ExampleCount
FROM 
    VocabularyCategories c
    LEFT JOIN VocabularyWords w ON c.Id = w.CategoryId
    LEFT JOIN VocabularyExamples e ON w.Id = e.WordId
GROUP BY 
    c.Id, c.Name, w.Id, w.Word
ORDER BY 
    c.Id, w.Id;
```

### Get Word with All Examples

```sql
SELECT 
    w.Word,
    w.Meaning,
    e.Sentence,
    e.Translation
FROM 
    VocabularyWords w
    LEFT JOIN VocabularyExamples e ON w.Id = e.WordId
WHERE 
    w.Id = 1
ORDER BY 
    e.DisplayOrder;
```

### Check Orphaned Records

```sql
-- Words without valid Category
SELECT * FROM VocabularyWords w
WHERE NOT EXISTS (
    SELECT 1 FROM VocabularyCategories c WHERE c.Id = w.CategoryId
);

-- Examples without valid Word
SELECT * FROM VocabularyExamples e
WHERE NOT EXISTS (
    SELECT 1 FROM VocabularyWords w WHERE w.Id = e.WordId
);
```

## ✅ Verification Checklist

- [x] **Primary Keys** tồn tại trên cả 3 bảng
- [x] **Foreign Keys** được định nghĩa đúng:
  - [x] VocabularyWords.CategoryId → VocabularyCategories.Id
  - [x] VocabularyExamples.WordId → VocabularyWords.Id
- [x] **Cascade Delete** được cấu hình:
  - [x] Delete Category → Delete Words → Delete Examples
  - [x] Delete Word → Delete Examples
- [x] **Indexes** tồn tại trên foreign key columns
- [x] **Unique Constraints** prevent duplicates
- [x] **EF Core Configuration** match database schema
- [x] **No Orphaned Records** trong data hiện tại

## 🎯 Summary

**Relationships Status: ✅ CORRECT**

Tất cả các bảng đã được liên kết đúng với:
- ✅ Primary Keys
- ✅ Foreign Keys with CASCADE DELETE
- ✅ Proper Indexes
- ✅ Unique Constraints
- ✅ Data Integrity Protection

**Cascade Flow:**
```
Category (Delete) 
    ↓ CASCADE
Words (Auto Delete)
    ↓ CASCADE  
Examples (Auto Delete)
```

Hệ thống database đã sẵn sàng và relationships được thiết lập đúng chuẩn! 🚀
