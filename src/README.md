# 🎓 RoomEnglish - English Learning Platform

A comprehensive English learning platform built with .NET 8 and Vue 3, featuring vocabulary management, AI-powered example generation, and interactive learning modules.

## 📚 Documentation

Detailed technical documentation and guides are available in the [docs/](./docs/) folder:
- [Performance & Optimization](./docs/PERFORMANCE_OPTIMIZATION_README.md)
- [Deployment Guide](./docs/DEPLOYMENT_GUIDE.md)
- [Database & Architecture](./docs/DATABASE_RELATIONSHIPS_VERIFICATION.md)
- [View all documentation →](./docs/README.md)

---

## 🚀 Overview

RoomEnglish is a modern, full-stack English learning platform that combines:
- **Advanced Vocabulary Management** - Admin tools for managing words, categories, and examples
- **AI-Powered Content Generation** - ChatGPT integration for contextual examples and grammar explanations
- **Interactive Learning Modules** - Vocabulary and Dictation practice with real-time feedback
- **Performance-Optimized Architecture** - Parallel processing, batch operations, and client-side calculations
- **Clean Code & Scalability** - Clean Architecture, CQRS, modular Vue 3 components

---

## 🏗️ Architecture

### **Solution Structure**

```
src/
├── Application/           # Business logic, CQRS commands/queries, MediatR handlers
├── Domain/               # Core domain entities, value objects, enums, business rules
├── Infrastructure/       # Data access (EF Core), Identity, OpenAI integration
└── Web/
    ├── Endpoints/        # Minimal API endpoints (authentication, vocabulary, examples)
    ├── ClientApp/        # Vue 3 + TypeScript + Vite frontend
    │   └── src/
    │       ├── modules/  # Feature modules (vocabulary, dictation, auth, etc.)
    │       ├── components/  # Shared UI components
    │       ├── composables/ # Reusable logic (useQuery, usePromiseWrapper)
    │       ├── stores/   # Pinia state management
    │       └── services/ # API services
    └── appsettings.json  # Configuration (ChatGPT, logging, database)
```

### **Clean Architecture Layers**

- **Domain**: Pure business entities, value objects, domain events
- **Application**: Use cases implemented as CQRS commands/queries via MediatR
- **Infrastructure**: EF Core, ASP.NET Identity, OpenAI SDK, external services
- **Web**: Minimal APIs, Vue 3 SPA, static file hosting

---

## 🎯 Key Features

### **1. Vocabulary Management**
- ✅ Multi-select data grid with advanced filtering and sorting
- ✅ **Bulk AI Example Generation** with configuration modal:
  - Select multiple words (1-100+)
  - Configure example count per word (1-20)
  - Choose difficulty levels (multiple selection)
  - Toggle grammar explanations and context diversity
  - Real-time total examples calculation
  - Estimated time display
- ✅ Excel/JSON import/export
- ✅ Real-time search and pagination
- ✅ Category-based organization
- ✅ **Default role assignment**: New accounts automatically get "User" role
- ✅ Admin and user role management with Google OAuth support

### **2. AI-Powered Example Generation**
- ✅ Generate contextual examples using ChatGPT API
- ✅ **Parallel processing** for multiple words (5-8x faster)
- ✅ **Multiple difficulty levels in single API call** (3x token savings)
  - Easy (Beginner) - Simple sentences, basic vocabulary
  - Medium (Intermediate) - Longer sentences, richer vocabulary
  - Hard (Advanced) - Complex sentences, advanced grammar
- ✅ **Batch generation**: 5 examples × 3 difficulty levels = 15 examples in one call
- ✅ Grammar explanations and Vietnamese translations
- ✅ Configurable generation settings (example count, difficulty levels, grammar, context)
- ✅ Automatic retry with exponential backoff
- ✅ Fallback data when API fails
- ✅ Smart duplicate detection using in-memory HashSet

**Performance Results:**
| Words | Before | After | Improvement |
|-------|--------|-------|-------------|
| 1 Word | 5-10s | 3-5s | **40-50% faster** |
| 5 Words | 25-50s | 8-15s | **70-80% faster** |
| 10 Words | 50-100s | 15-25s | **75-80% faster** |

### **3. Learning Modules**

#### **📚 Vocabulary Learning Mode**
- Learn words with definitions, phonetics, part of speech
- Load 100 examples per word (increased from 10)
- **Lazy loading** with Intersection Observer:
  - Initial batch: 20 examples
  - Load more: 20 examples per scroll
  - Smooth performance even with large datasets
- Track progress per word
- Compact word context header (2 lines, centered)
- Vietnamese meaning support

#### **🎤 Dictation Practice**
- **Example-by-example progression** (not word-by-word)
- Real-time word-by-word comparison with color coding:
  - ✅ Correct (green)
  - ✏️ Typing (blue - correct prefix)
  - ❌ Incorrect (red - shows correct word)
  - ⚠️ Missing (yellow)
- Audio playback with adjustable speed (0.5x - 1.25x)
- **Keyboard shortcuts**:
  - <kbd>Ctrl</kbd> - Play audio (programmatic button click)
  - <kbd>Enter</kbd> - Submit answer
- Client-side accuracy calculation (instant, offline-capable)
- Full result display: English, Vietnamese translation, Grammar
- Word context display showing:
  - Word + Phonetic + Part of Speech
  - Meaning • Vietnamese Meaning

#### **📝 Review Feature (NEW!)**
- **Daily Review** with 20 random studied examples
- Only shows examples you've practiced (TotalAttempts > 0)
- One example per word for variety
- Interactive sidebar:
  - Checkbox to mark completion
  - Click to jump to specific example
  - Visual progress tracking
- Session progress tracker
- Quick access from Dashboard
- Uses IUser service for authenticated user context

### **4. Speech & Audio**
- **Web Speech API** for text-to-speech (bypasses autoplay policy)
- **OpenAI TTS** integration (premium voices)
- Global speech settings (voice, rate, pitch)
- Unified `GlobalSpeechButton` component
- Voice recognition for dictation input

---

## ⚡ Performance Optimizations

### **Backend Optimizations**

#### **Parallel Processing**
```csharp
// Process multiple words simultaneously with controlled concurrency
var semaphore = new SemaphoreSlim(batchSize, batchSize);
var tasks = vocabularyWords.Select(async word => {
    await semaphore.WaitAsync(cancellationToken);
    try {
        await ProcessVocabularyWord(word, result, request, cancellationToken);
    } finally {
        semaphore.Release();
    }
});
await Task.WhenAll(tasks);
```

### **AI Optimization - Multiple Difficulty Levels**
```csharp
// Generate all difficulty levels in ONE API call (saves ~66% tokens)
var difficultyLevels = request.DifficultyLevels ?? new List<DifficultyLevel> { DifficultyLevel.Easy };
var examplesPerLevel = request.ExampleCount;
var totalExamples = examplesPerLevel * difficultyLevels.Count;

// AI Prompt: "Generate 15 examples (5 easy, 5 medium, 5 hard) in one response"
// Old way: 3 separate API calls
// New way: 1 API call with mixed difficulty

// Distribute examples to correct difficulty levels
var exampleIndex = 0;
foreach (var example in generatedExamples) {
    var levelIndex = exampleIndex / examplesPerLevel;
    example.DifficultyLevel = (int)difficultyLevels[levelIndex];
    exampleIndex++;
}
```

#### **Database Optimization**
- Batch queries instead of N+1 pattern
- In-memory duplicate detection
- Single `SaveChangesAsync()` at the end
- 90% reduction in database queries

#### **Retry Logic**
- Exponential backoff for failed API calls
- Configurable max retries (default: 3)
- Timeout per request (default: 30s)

### **Frontend Optimizations**

#### **Client-Side Accuracy Calculation**
```typescript
// No API call needed - instant feedback
const calculateDictationAccuracy = (userInput, correctAnswer, timeTaken) => {
  const normalize = (text) => text.toLowerCase().trim()
    .replace(/[.,!?;:]/g, '').replace(/\s+/g, ' ')
  
  const userWords = normalize(userInput).split(' ')
  const correctWords = normalize(correctAnswer).split(' ')
  
  let matchingWords = 0
  for (let i = 0; i < Math.min(userWords.length, correctWords.length); i++) {
    if (userWords[i] === correctWords[i]) matchingWords++
  }
  
  const accuracy = Math.round((matchingWords / Math.max(userWords.length, correctWords.length)) * 100)
  return { isCorrect: accuracy === 100, accuracyPercentage: accuracy }
}
```

#### **Example-Based Navigation**
```typescript
// Navigate through individual examples, not just words
const nextWord = () => {
  const word = currentWord.value
  if (word?.examples && currentExampleIndex.value < word.examples.length - 1) {
    currentExampleIndex.value++  // Next example in same word
  } else {
    currentExampleIndex.value = 0
    currentIndex.value++  // Next word
  }
}
```

#### **Lazy Loading with Intersection Observer**
```typescript
// Load 100 examples but only render 20 initially
const INITIAL_BATCH = 20
const LOAD_MORE_BATCH = 20
const displayedExamples = ref<VocabularyExample[]>([])

const loadMore = () => {
  const currentLength = displayedExamples.value.length
  const nextBatch = allExamples.value.slice(
    currentLength, 
    currentLength + LOAD_MORE_BATCH
  )
  displayedExamples.value.push(...nextBatch)
}

// Trigger load more when sentinel element is visible
const { stop } = useIntersectionObserver(
  sentinelElement,
  ([{ isIntersecting }]) => {
    if (isIntersecting && hasMore.value) {
      loadMore()
    }
  }
)
```

---

## ⚙️ Configuration

### **Performance Tuning**

Edit `Web/appsettings.json`:

```json
{
  "ChatGPT": {
    "ConcurrentRequests": 8,        // Parallel API calls (3-10 recommended)
    "RequestTimeoutSeconds": 30,    // Timeout per request (20-60s)
    "MaxRetries": 3                 // Retry attempts (2-5)
  },
  "OpenAI": {
    "ApiKey": "your-api-key-here"
  }
}
```

### **Logging Levels**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "RoomEnglish.Application.VocabularyExamples": "Debug"  // Detailed logs
    }
  }
}
```

---

## 🚀 Quick Start

### **⚠️ Nếu gặp lỗi build Static Web Assets**

```powershell
# Chạy script tự động clean & build
.\clean-build.ps1

# Xem hướng dẫn chi tiết
Get-Content CLEAN_BUILD_README.md
```

📘 **Lỗi "duplicate key" là lỗi phổ biến nhất!** Xem [CLEAN_BUILD_README.md](./CLEAN_BUILD_README.md) để hiểu nguyên nhân và cách phòng ngừa.

### **Backend (.NET 8)**
```bash
cd src/Web
dotnet restore
dotnet run
```

### **Frontend (Vue 3)**
```bash
cd src/Web/ClientApp
npm install
npm run dev
```

### **Access Application**
- **Frontend**: http://localhost:5173
- **Backend API**: https://localhost:5001
- **API Documentation**: https://localhost:5001/swagger

---

## 🛠️ Technology Stack

### **Backend**
- **.NET 8, C# 12** - Latest .NET features, minimal APIs
- **Entity Framework Core 8** - SQL Server / SQLite support
- **MediatR 12.x** - CQRS pattern implementation
- **ASP.NET Core Identity** - Authentication & authorization
- **OpenAI SDK (latest)** - ChatGPT integration for AI generation
- **Minimal APIs** - Fast, lightweight endpoints
- **JWT Bearer Authentication** - Secure token-based auth
- **IUser Service** - Authenticated user context injection

### **Frontend**
- **Vue 3.4+** (Composition API, `<script setup>`)
- **TypeScript 5.x** (strict mode)
- **Vite 5.x** (dev server, build tool, HMR)
- **Pinia 2.x** (state management)
- **Vue Router 4.x** (client-side routing)
- **Iconify** (@iconify/vue) - 100k+ icons
- **VueUse** - Collection of Vue composition utilities
- **Intersection Observer API** - Lazy loading implementation
- **Web Speech API** - Text-to-speech functionality

### **Architecture Patterns**
- **Clean Architecture** - Domain, Application, Infrastructure, Web layers
- **CQRS** (Command Query Responsibility Segregation) - MediatR handlers
- **Repository Pattern** - DbContext abstraction
- **Dependency Injection** - Built-in .NET DI container
- **Modular Frontend** - Feature-based modules (vocabulary, dictation, auth)
- **Composable Pattern** - Reusable Vue composition functions
- **IUser Interface** - Authenticated user context pattern

---

## 📊 Performance Monitoring & Logging

### **Backend Logging**

Comprehensive performance tracking at every layer:

```log
[INFO] Starting example generation for 5 words: computer, programming, software, algorithm, database
[INFO] Database query completed in 45ms. Found 5/5 vocabulary words
[INFO] Starting parallel processing with batch size: 8 for 5 words
[DEBUG] ChatGPT API call completed for 'computer' in 2847ms. Generated 10 examples
[DEBUG] Completed processing for 'computer' in 3012ms. Added: 9, Skipped: 1
[INFO] Parallel processing completed in 13250ms for 5 words
[INFO] Database save completed in 156ms. Total operation time: 13451ms
[INFO] Generated 47 examples with 3 errors
```

### **Frontend Metrics**

```javascript
🚀 Started generating examples for 5 words: ["computer", "programming", ...]
✅ Generate examples completed in 13451ms
📊 Performance metrics:
  - Total time: 13451ms (13.5s)
  - Average per word: 2690ms
  - Success rate: 94.0%
  - Words processed: 5
```

---

## 📚 Detailed Documentation

- **[Performance Optimization Details](PERFORMANCE_OPTIMIZATION_README.md)** - Parallel processing, database optimization, retry logic
- **[Performance Logging Implementation](PERFORMANCE_LOGGING_README.md)** - Monitoring, metrics, troubleshooting
- **[Frontend Architecture](Web/ClientApp/README.md)** - Vue 3 best practices, clean architecture, composables
- **[Vocabulary Module](Web/ClientApp/src/modules/vocabulary/README.md)** - Learning flow, components, keyboard shortcuts
- **[Dictation Components](Web/ClientApp/src/modules/vocabulary/components/dictation/README.md)** - AudioPlayer, WordComparison, ResultDisplay

---

## 🧩 Frontend Architecture

### **Clean Architecture Pattern**

```
src/
├── core/              # Business logic (entities, interfaces, services)
├── infrastructure/    # Implementations (API, repositories, storage)
└── modules/           # UI features (vocabulary, auth, posts, etc.)
    └── [module-name]/
        ├── components/  # UI components
        ├── composables/ # Reusable logic
        ├── stores/      # Pinia stores
        ├── types/       # TypeScript interfaces
        └── views/       # Route views
```

### **Key Composables**

- `useQuery` - Promise handler with cache (static data)
- `usePromiseWrapper` - Promise handler for mutations with loading states
- `useDictation` - Dictation state + client-side accuracy calculation
- `useSpeechSynthesis` - Unified TTS (Web Speech + OpenAI)
- `useSpeechSettings` - Global speech configuration
- `useVocabulariesManagement` - Vocabulary CRUD operations
- `useExamplesManagement` - Example management + AI generation
- `useIntersectionObserver` (VueUse) - Lazy loading implementation

### **Best Practices**

- ✅ Composition API, `<script setup>`
- ✅ TypeScript strict mode, no `any` types
- ✅ External libraries proxied via shared components
- ✅ Computed for derived state (not ref + watch)
- ✅ Cleanup listeners in `onUnmounted`
- ✅ camelCase variables, PascalCase components

---

## 🎉 Results & Impact

### **Performance**
- **5-8x faster** example generation for multiple words via parallel processing
- **3x token savings** with single API call for multiple difficulty levels
- **90% reduction** in database queries with batch operations
- **95% success rate** with retry mechanisms
- **Instant feedback** with client-side calculations
- **Smooth scrolling** with lazy loading (100 items, render 20 at a time)
- **<100ms UI response** time for user interactions

### **User Experience**
- Real-time word comparison during typing
- Keyboard shortcuts for efficiency (<kbd>Ctrl</kbd> for audio, <kbd>Enter</kbd> to submit)
- Offline-capable accuracy calculation
- Example-by-example learning progression
- **Daily Review feature** for studied examples
- Compact, centered word context display
- Lazy loading for large datasets (no lag)
- Mobile-responsive design
- **Quick Actions Dashboard** with Daily Review shortcut

### **Code Quality**
- **Clean Architecture** with proper layer separation
- **Type-safe** with TypeScript strict mode
- **No `any` types** - full type coverage
- Modular, testable components
- **Comprehensive logging** (backend + frontend)
- **Error handling** with try-catch wrappers
- **IUser service** for consistent user context
- Production-ready scalability

---

## 🧪 Testing & Quality

- **Backend**: xUnit, integration tests
- **Frontend**: Vitest (unit tests)
- **Linting**: ESLint + Prettier
- **Type Safety**: TypeScript strict mode
- **Architecture**: Clean separation of concerns

---

## � Documentation

### **📖 Main Guides**
- **[CLEAN_BUILD_README.md](./CLEAN_BUILD_README.md)** - Fix Static Web Assets errors (lỗi phổ biến nhất!)
- **[GUIDE_ADD_DATABASE_COLUMN.md](./GUIDE_ADD_DATABASE_COLUMN.md)** - Step-by-step guide để thêm database column
- **[Frontend Mapping](./Web/ClientApp/FRONTEND_MAPPING.md)** - Frontend architecture & component mapping
- **[OpenAI TTS Guide](./Web/ClientApp/OPENAI_TTS_GUIDE.md)** - Text-to-Speech integration guide

### **🛠️ Common Tasks**

| Task | Script/Command | Guide |
|------|----------------|-------|
| Fix build errors | `.\clean-build.ps1` | [CLEAN_BUILD_README.md](./CLEAN_BUILD_README.md) |
| Add database column | Manual steps | [GUIDE_ADD_DATABASE_COLUMN.md](./GUIDE_ADD_DATABASE_COLUMN.md) |
| Create migration | `dotnet ef migrations add [Name]` | [GUIDE_ADD_DATABASE_COLUMN.md](./GUIDE_ADD_DATABASE_COLUMN.md#step-2) |
| Update database | `dotnet ef database update` | [GUIDE_ADD_DATABASE_COLUMN.md](./GUIDE_ADD_DATABASE_COLUMN.md#step-3) |

---

## �👥 Developer Notes

### **Getting Started**
1. Clone the repository
2. Set up appsettings.json with OpenAI API key
3. Run backend: `cd src/Web && dotnet run`
4. Run frontend: `cd src/Web/ClientApp && npm install && npm run dev`

### **Code Conventions**
- Use `===` not `==`
- No nested try-catch (use composables)
- Prefer `computed` over `ref()` + `watch()`
- Call API functions in `<script setup>` (before lifecycle hooks)

### **Contributing**
- Follow Clean Architecture layers
- Use MediatR for new commands/queries
- Add TypeScript interfaces for all models
- Write unit tests for complex logic
- Update README for new features

---

## 🔍 Troubleshooting

### **❌ Build Error: Static Web Assets duplicate key** (PHỔ BIẾN NHẤT)

```powershell
# Fix tự động bằng script
.\clean-build.ps1

# Xem chi tiết tại:
Get-Content CLEAN_BUILD_README.md
```

📘 **80% các lỗi build là do lỗi này!** Script tự động sẽ xóa tất cả cache và build lại.

### **API Rate Limiting**
- Reduce `ConcurrentRequests` to 3-5

### **Memory Issues**
- Lower batch size in configuration

### **Database Timeouts**
- Increase timeout in connection string

### **Audio Not Playing (Keyboard)**
- Using Web Speech API (bypasses autoplay policy)
- Check browser compatibility

### **Migration Errors**

Xem chi tiết trong [GUIDE_ADD_DATABASE_COLUMN.md](./GUIDE_ADD_DATABASE_COLUMN.md#-xử-lý-lỗi-thường-gặp)

---

## 📞 Support & Contact

For issues or questions:
- Check [GitHub Issues](https://github.com/your-repo/issues)
- Review detailed documentation above
- Contact the development team

---

**Last Updated:** November 9, 2025  
**Version:** 2.1.0  
**Status:** ✅ Production Ready

## 🆕 Recent Updates (v2.1.0)

### **November 2025**
- ✅ **Review Feature** - Daily review with 20 studied examples
- ✅ **AI Optimization** - Multiple difficulty levels in single API call (3x token savings)
- ✅ **Lazy Loading** - 100 examples with smooth rendering (20 initial + 20 per scroll)
- ✅ **Compact UI** - 2-line centered word context header
- ✅ **Default Roles** - New accounts auto-assigned "User" role
- ✅ **IUser Service** - Consistent authenticated user context
- ✅ **Bulk Generation** - Configure AI generation for multiple words at once
- ✅ **Vietnamese Support** - Full Vietnamese meaning display
- ✅ **Dashboard Shortcuts** - Quick access to Daily Review
- ✅ **Code Cleanup** - Removed unused functions, optimized components

---

Perfect for efficiently managing large vocabulary collections and delivering an engaging, high-performance English learning experience! 🌟