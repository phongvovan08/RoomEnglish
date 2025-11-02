# Audio Caching Flow - Text-to-Speech

## Tổng quan
Hệ thống sử dụng **2 tầng cache** để tối ưu hiệu năng khi phát audio:
1. **Memory Cache** (RAM) - Cực nhanh, mất khi tắt trình duyệt
2. **Database Cache** (SQL Server) - Lâu dài, chia sẻ giữa các user

## Flow khi bấm Listen Button

```
User clicks Listen
       ↓
GlobalSpeechButton.vue
       ↓
useSpeechSynthesis.speak()
       ↓
┌─────────────────────────────────────┐
│  1. Check Memory Cache (Map)        │ ← Fastest (RAM)
│     Key: text_voice_speed           │
└─────────────────────────────────────┘
       ↓ Not Found
┌─────────────────────────────────────┐
│  2. Check Database Cache (SQL)      │ ← Fast (HTTP call)
│     GET /api/audio-cache            │
│     Query: text, voice, rate        │
└─────────────────────────────────────┘
       ↓ Not Found
┌─────────────────────────────────────┐
│  3. Call OpenAI API                 │ ← Slow (External API)
│     POST https://api.openai.com/... │
│     Model: tts-1                    │
│     Voice: alloy/nova/etc.          │
│     Speed: 1.0 (always)             │
└─────────────────────────────────────┘
       ↓ Get Audio Data
┌─────────────────────────────────────┐
│  4. Save to Caches                  │
│     - Memory Cache (instant)        │
│     - Database Cache (async)        │
│       POST /api/audio-cache         │
└─────────────────────────────────────┘
       ↓
┌─────────────────────────────────────┐
│  5. Play Audio                      │
│     HTMLAudioElement.play()         │
│     playbackRate = user setting     │
└─────────────────────────────────────┘
```

## Chi tiết từng tầng

### 1. Memory Cache (Frontend)
**Location**: `useSpeechSynthesis.ts`
```typescript
const memoryCache = ref(new Map<string, Blob>())
```

**Đặc điểm**:
- ✅ Cực nhanh (< 1ms)
- ✅ Không tốn bandwidth
- ❌ Mất khi reload trang
- ❌ Không chia sẻ giữa các tab

**Cache Key**: `${text}_${voice}_${speed}`
Ví dụ: `"Hello world_alloy_1.0"`

### 2. Database Cache (Backend)
**Location**: SQL Server → Table `AudioCache`

**Schema**:
```sql
CREATE TABLE AudioCache (
    Id              INT PRIMARY KEY,
    Text            NVARCHAR(MAX),      -- Câu cần đọc
    Voice           NVARCHAR(50),       -- alloy, nova, etc.
    Rate            FLOAT,              -- 1.0 (luôn luôn)
    Provider        NVARCHAR(50),       -- openai
    AudioData       VARBINARY(MAX),     -- MP3 data
    MimeType        NVARCHAR(50),       -- audio/mpeg
    SizeBytes       INT,                -- Kích thước file
    CacheKeyHash    NVARCHAR(100),      -- Hash để tìm nhanh
    HitCount        INT,                -- Số lần dùng
    LastAccessedAt  DATETIME2,          -- Lần cuối dùng
    ExpiresAt       DATETIME2,          -- Thời hạn (30 ngày)
    Created         DATETIME2,
    CreatedBy       NVARCHAR(255),
    LastModified    DATETIME2,
    LastModifiedBy  NVARCHAR(255)
)
```

**Đặc điểm**:
- ✅ Nhanh (10-50ms)
- ✅ Lâu dài (30 ngày)
- ✅ Chia sẻ cho tất cả users
- ✅ Giảm API calls đáng kể
- ❌ Tốn storage (~50KB/câu)

**Flow lưu cache**:
```csharp
// Application/Audio/Commands/SaveAudioCacheCommand.cs
public async Task<Unit> Handle(SaveAudioCacheCommand request, ...)
{
    var cacheKey = $"{request.Text}_{request.Voice}_{request.Rate}_{request.Provider}";
    var hash = ComputeHash(cacheKey);
    
    var cache = new AudioCache
    {
        Text = request.Text,
        Voice = request.Voice,
        Rate = request.Rate,
        Provider = request.Provider,
        AudioData = request.AudioData,        // byte[]
        MimeType = request.MimeType,
        SizeBytes = request.AudioData.Length,
        CacheKeyHash = hash,
        HitCount = 0,
        LastAccessedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiryDays ?? 30)
    };
    
    _context.AudioCaches.Add(cache);
    await _context.SaveChangesAsync();
}
```

**Flow lấy cache**:
```csharp
// Application/Audio/Queries/GetAudioCacheQuery.cs
public async Task<AudioCacheDto?> Handle(GetAudioCacheQuery request, ...)
{
    var cacheKey = $"{request.Text}_{request.Voice}_{request.Rate}_{request.Provider}";
    var hash = ComputeHash(cacheKey);
    
    var cache = await _context.AudioCaches
        .Where(c => c.CacheKeyHash == hash 
                 && (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow))
        .FirstOrDefaultAsync();
    
    if (cache != null)
    {
        // Update stats
        cache.HitCount++;
        cache.LastAccessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return new AudioCacheDto
        {
            AudioData = cache.AudioData,
            MimeType = cache.MimeType
        };
    }
    
    return null; // Not found → 404
}
```

### 3. OpenAI API (External)
**Endpoint**: `https://api.openai.com/v1/audio/speech`

**Request**:
```json
{
  "model": "tts-1",
  "voice": "alloy",
  "input": "The cat sat on the mat",
  "speed": 1.0
}
```

**Response**: Binary MP3 data (~50KB cho 1 câu)

**Đặc điểm**:
- ❌ Chậm (500-2000ms)
- ❌ Tốn tiền ($0.015 / 1000 characters)
- ❌ Cần API key
- ✅ Chất lượng tốt
- ✅ Nhiều giọng nói

## Chiến lược Cache

### Tại sao luôn cache với speed = 1.0?
**Lý do**: Tiết kiệm storage và API calls

**Giải pháp**: Dùng `HTMLAudioElement.playbackRate` để thay đổi tốc độ trong browser

**Code**:
```typescript
// useSpeechSynthesis.ts
const speakWithOpenAI = async (text: string, ...) => {
    const normalSpeed = 1.0  // Always request 1.0
    
    // Cache key uses normalSpeed
    const cacheKey = `${text}_${voice}_${normalSpeed}`
    
    // ... get audio ...
    
    // Play with user's preferred speed
    const audio = new Audio(url)
    audio.playbackRate = options.rate || 1.0  // 0.5x - 2.0x
    audio.play()
}
```

**Lợi ích**:
- 1 file MP3 → Dùng cho tất cả tốc độ (0.5x, 0.75x, 1x, 1.25x, 1.5x, 2x)
- Giảm 6x storage
- Giảm 6x API calls
- Giảm 6x tiền OpenAI

## Performance Numbers

### Lần đầu phát audio (cache miss):
```
Memory Cache:      0ms    (miss)
Database Cache:    0ms    (miss)
OpenAI API:     1500ms    ← User chờ
Total:          1500ms
```

### Lần 2 trong cùng session (memory hit):
```
Memory Cache:      1ms    ✅ (hit)
Total:             1ms    (150x faster!)
```

### Lần 2 sau khi reload trang (database hit):
```
Memory Cache:      0ms    (miss)
Database Cache:   20ms    ✅ (hit)
Total:            20ms    (75x faster!)
```

### Lần 2 ở user khác (database hit):
```
Memory Cache:      0ms    (miss - khác browser)
Database Cache:   20ms    ✅ (hit - shared)
Total:            20ms    (User không tốn API call!)
```

## Cache Statistics API

### GET /api/audio-cache/stats
```json
{
  "totalEntries": 342,
  "totalSizeBytes": 17520000,      // ~17MB
  "expiredEntries": 12,
  "oldestEntry": "2025-01-15",
  "newestEntry": "2025-10-26",
  "totalHits": 4521,               // Đã tiết kiệm 4521 API calls!
  "averageSizeKB": 51.2
}
```

### POST /api/audio-cache/cleanup
Xóa cache cũ/hết hạn để tiết kiệm storage:
```json
{
  "maxCacheSizeMB": 100,    // Giữ tối đa 100MB
  "deleteExpired": true     // Xóa entries hết hạn
}
```

## Ví dụ thực tế

### Câu: "The cat sat on the mat"
**Lần 1 - User A**:
1. Memory: miss (0ms)
2. Database: miss (0ms)
3. OpenAI: call API (1500ms) → Lưu vào database
4. **Total: 1500ms**

**Lần 2 - User A (same session)**:
1. Memory: **HIT** (1ms)
2. **Total: 1ms** ✅

**Lần 3 - User B (khác browser)**:
1. Memory: miss (0ms)
2. Database: **HIT** (20ms) → Lưu vào memory của User B
3. **Total: 20ms** ✅

**Lần 4 - User B (same session)**:
1. Memory: **HIT** (1ms)
2. **Total: 1ms** ✅

### Kết quả
- 4 lần phát = **1 API call** thay vì 4 calls
- Tiết kiệm: **75% API cost**
- Tốc độ: **1-20ms** thay vì 1500ms

## Maintenance

### Auto Cleanup
Cache tự động cleanup theo:
1. **ExpiresAt**: Xóa sau 30 ngày
2. **LRU** (Least Recently Used): Xóa entries ít dùng nhất khi quá 100MB
3. **HitCount**: Giữ lại entries được dùng nhiều

### Manual Cleanup
Admin có thể gọi:
```bash
POST /api/audio-cache/cleanup
{
  "maxCacheSizeMB": 50,
  "deleteExpired": true
}
```

## Code Flow Summary

```typescript
// Frontend: useSpeechSynthesis.ts
speak(text, options) {
  // 1. Check memory
  if (memoryCache.has(key)) return memoryCache.get(key)
  
  // 2. Check database
  const dbCache = await getCachedAudio(text, voice, 1.0, 'openai')
  if (dbCache) {
    memoryCache.set(key, dbCache)  // Save to memory
    return dbCache
  }
  
  // 3. Call OpenAI
  const audio = await openai.tts.create({ text, voice, speed: 1.0 })
  
  // 4. Save to caches
  memoryCache.set(key, audio)              // Instant
  saveAudioToCache(text, voice, 1.0, ...)  // Async
  
  // 5. Play with custom speed
  audioElement.playbackRate = options.rate
  audioElement.play()
}
```

```csharp
// Backend: GetAudioCacheQuery.cs
public async Task<AudioCacheDto?> Handle(GetAudioCacheQuery query)
{
    var hash = ComputeHash($"{query.Text}_{query.Voice}_{query.Rate}_{query.Provider}");
    
    var cache = await _context.AudioCaches
        .Where(c => c.CacheKeyHash == hash && !c.IsExpired)
        .FirstOrDefaultAsync();
    
    if (cache != null)
    {
        cache.HitCount++;
        cache.LastAccessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
    
    return cache?.ToDto();
}
```

## Kết luận

**Mỗi lần bấm Listen**:
1. ✅ Kiểm tra Memory Cache (1ms)
2. ✅ Kiểm tra Database Cache (20ms)
3. ❌ Gọi OpenAI API (1500ms) - Chỉ lần đầu
4. ✅ Lưu vào cả 2 tầng cache
5. ✅ Phát audio với tốc độ tùy chỉnh

**Lợi ích**:
- 🚀 **150x faster** cho lần phát tiếp theo
- 💰 **Tiết kiệm 75-90% API cost**
- 🌍 **Chia sẻ cache** giữa tất cả users
- ⚡ **Instant playback** với memory cache
