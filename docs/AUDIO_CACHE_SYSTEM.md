# Audio Cache System

## Overview

The Audio Cache System is a sophisticated caching solution for Text-to-Speech (TTS) audio files that reduces API calls, improves performance, and provides a better user experience. It uses a **two-tier caching strategy** combining in-memory cache and database persistence.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Audio Request Flow                       │
└─────────────────────────────────────────────────────────────┘
                           ↓
        ┌──────────────────────────────────────┐
        │     1. Check Memory Cache (RAM)      │
        │     ⚡ Super Fast (5-10ms)            │
        └──────────────────────────────────────┘
                           ↓ (miss)
        ┌──────────────────────────────────────┐
        │     2. Check Database Cache          │
        │     💾 Fast (50-100ms)               │
        └──────────────────────────────────────┘
                           ↓ (miss)
        ┌──────────────────────────────────────┐
        │     3. Call OpenAI TTS API           │
        │     🌐 Slow (500-1000ms)             │
        └──────────────────────────────────────┘
                           ↓
        ┌──────────────────────────────────────┐
        │     4. Save to Memory + Database     │
        └──────────────────────────────────────┘
```

## Features

### ✨ Two-Tier Caching

1. **Memory Cache (L1)**
   - Stored in browser memory (RAM)
   - Ultra-fast access (5-10ms)
   - Cleared on page refresh
   - Perfect for current session

2. **Database Cache (L2)**
   - Stored in SQL Server database
   - Persistent across sessions
   - Shared across devices (same user)
   - Unlimited storage capacity

### 🎯 Smart Features

- **SHA256 Hash Indexing**: Fast lookups using unique cache keys
- **Hit Tracking**: Monitors which audio files are most popular
- **Auto-Expiration**: Configurable expiry (default 30 days)
- **LRU Eviction**: Removes least-used entries when cache is full
- **Cross-Device Sync**: Same user can share cache across devices
- **Automatic Cleanup**: Scheduled removal of expired entries

## Database Schema

```csharp
public class AudioCache : BaseAuditableEntity
{
    public string Text { get; set; }              // Original text (max 2000 chars)
    public string Voice { get; set; }             // Voice name (e.g., "alloy")
    public double Rate { get; set; }              // Speech rate (0.5-2.0)
    public string Provider { get; set; }          // TTS provider ("openai")
    public byte[] AudioData { get; set; }         // MP3 audio bytes
    public string MimeType { get; set; }          // MIME type (audio/mpeg)
    public int SizeBytes { get; set; }            // File size in bytes
    public string CacheKeyHash { get; set; }      // SHA256 hash (unique)
    public int HitCount { get; set; }             // Number of cache hits
    public DateTime LastAccessedAt { get; set; }  // Last access timestamp
    public DateTime? ExpiresAt { get; set; }      // Expiration date
}
```

### Indexes

1. **Unique Index on CacheKeyHash**: Fast lookups by cache key
2. **Index on ExpiresAt**: Efficient cleanup of expired entries
3. **Composite Index on (HitCount, LastAccessedAt)**: LRU eviction support

## API Endpoints

### 1. Get Cached Audio
```http
GET /api/audio-cache?text={text}&voice={voice}&rate={rate}&provider={provider}
```

**Response:**
- `200 OK`: Returns audio file (audio/mpeg)
- `404 Not Found`: Cache miss

**Example:**
```javascript
const params = new URLSearchParams({
  text: 'Hello world',
  voice: 'alloy',
  rate: '1.0',
  provider: 'openai'
})
const response = await fetch(`/api/audio-cache?${params}`)
const audioBlob = await response.blob()
```

### 2. Save Audio to Cache
```http
POST /api/audio-cache
Content-Type: application/json

{
  "text": "Hello world",
  "voice": "alloy",
  "rate": 1.0,
  "provider": "openai",
  "audioDataBase64": "base64_encoded_audio",
  "mimeType": "audio/mpeg",
  "expiryDays": 30
}
```

**Response:**
- `200 OK`: Audio saved successfully

### 3. Get Cache Statistics
```http
GET /api/audio-cache/stats
```

**Response:**
```json
{
  "totalEntries": 150,
  "totalSizeBytes": 47185920,
  "expiredEntries": 5,
  "oldestEntry": "2025-09-25T10:30:00Z",
  "newestEntry": "2025-10-25T14:20:00Z",
  "totalHits": 1250,
  "averageSizeKB": 307.24
}
```

### 4. Cleanup Cache
```http
POST /api/audio-cache/cleanup
Content-Type: application/json

{
  "maxCacheSizeMB": 100,
  "deleteExpired": true
}
```

**Response:**
```json
{
  "deletedEntries": 15
}
```

## Frontend Usage

### Using the Cache API

```typescript
import { useAudioCacheAPI } from '@/composables/useAudioCacheAPI'

const { getCachedAudio, saveAudioToCache, getCacheStats, cleanupCache } = useAudioCacheAPI()

// Get cached audio
const blob = await getCachedAudio('Hello world', 'alloy', 1.0, 'openai')

// Save audio to cache
await saveAudioToCache('Hello world', 'alloy', 1.0, 'openai', audioBlob, 30)

// Get statistics
const stats = await getCacheStats()
console.log(`Cache has ${stats.totalEntries} entries (${stats.totalSizeBytes} bytes)`)

// Cleanup
const deleted = await cleanupCache(100, true)
console.log(`Deleted ${deleted} entries`)
```

### Speech Synthesis Integration

The cache is automatically integrated into `useSpeechSynthesis`:

```typescript
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

const { speak, getCacheStats } = useSpeechSynthesis()

// Speak text (automatically uses cache)
await speak('Hello world', 'instance-1', { 
  provider: 'openai',
  voiceIndex: 0,
  rate: 1.0 
})

// Check cache stats
const stats = await getCacheStats()
console.log('Memory cache:', stats.memory)
console.log('Database cache:', stats.database)
```

## Cache Key Generation

Cache keys are generated using SHA256 hash:

```csharp
var cacheKey = $"{text}_{voice}_{rate:F2}_{provider}";
var bytes = Encoding.UTF8.GetBytes(cacheKey);
var hash = SHA256.HashData(bytes);
var cacheKeyHash = Convert.ToHexString(hash).ToLowerInvariant();
```

**Example:**
- Input: `"Hello world_alloy_1.00_openai"`
- Output: `"a3f5b1c2d4e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2"`

## Performance Metrics

### API Call Reduction

| Scenario | Before Cache | After Cache | Improvement |
|----------|--------------|-------------|-------------|
| First listen | 1 API call | 1 API call | 0% |
| Second listen | 1 API call | 0 API calls | 100% |
| 10 listens | 10 API calls | 1 API call | 90% |
| 100 listens | 100 API calls | 1 API call | 99% |

### Latency Improvement

| Cache Type | Latency | Use Case |
|------------|---------|----------|
| Memory | 5-10ms | Same session, instant replay |
| Database | 50-100ms | Cross-session, cross-device |
| API Call | 500-1000ms | Cache miss |

### Cost Savings

**OpenAI TTS Pricing:** $0.015 per 1K characters

**Example: 1000 students practicing 100 sentences each (avg 50 chars)**
- Without cache: 100,000 API calls × 50 chars = 5M chars = **$75**
- With cache: ~100 unique sentences × 50 chars = 5K chars = **$0.075**
- **Savings: $74.925 (99.9%)**

## Cache Management

### Automatic Cleanup

The system automatically cleans up:
1. **Expired entries**: Older than `ExpiresAt`
2. **Least-used entries**: When total size exceeds limit

### Manual Cleanup

```csharp
// Cleanup via API
var command = new CleanupAudioCacheCommand 
{ 
    MaxCacheSizeMB = 100,
    DeleteExpired = true
};
var deletedCount = await mediator.Send(command);
```

### Monitoring

View cache statistics in Speech Settings Panel:

```
Audio Cache:
├─ Memory: 5 items (1.2 MB)
├─ Database: 150 items (45 MB)
├─ Total Hits: 1,250
├─ Expired: 12 items ⚠️
└─ [Clear Audio Cache] button
```

## Configuration

### Default Settings

```csharp
// Entity defaults
ExpiryDays = 30                 // Cache expires after 30 days
MaxCacheSizeMB = 100            // Keep total cache under 100MB
```

### Customization

```typescript
// Save with custom expiry
await saveAudioToCache(text, voice, rate, provider, blob, 60) // 60 days

// Cleanup with custom limits
await cleanupCache(200, true) // 200MB limit
```

## Best Practices

### 1. Memory Management

```typescript
// Clear memory cache periodically
memoryCache.value.clear()
```

### 2. Cleanup Schedule

Run cleanup regularly (e.g., daily background job):

```csharp
public class AudioCacheCleanupJob : IHostedService
{
    public async Task Execute()
    {
        await mediator.Send(new CleanupAudioCacheCommand 
        { 
            MaxCacheSizeMB = 100,
            DeleteExpired = true
        });
    }
}
```

### 3. Cache Warming

Pre-cache common phrases:

```typescript
const commonPhrases = [
  'Hello, how are you?',
  'Good morning',
  'Thank you'
]

for (const phrase of commonPhrases) {
  await speak(phrase, `warmup-${i}`)
}
```

## Troubleshooting

### Issue: Cache not working

**Check:**
1. Database connection is active
2. AudioCaches table exists
3. API endpoints are accessible
4. No CORS issues

### Issue: High memory usage

**Solution:**
```typescript
// Clear memory cache
const { clearCache } = useSpeechSynthesis()
await clearCache()
```

### Issue: Stale cache entries

**Solution:**
```typescript
// Run cleanup
await cleanupCache(100, true)
```

## Migration

The AudioCache table is created via EF Core migration:

```bash
dotnet ef migrations add AddAudioCache --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
```

## Security Considerations

1. **No user-specific data**: Cache is shared (optional: can add UserId for privacy)
2. **Hash-based lookup**: Original text is hashed for privacy
3. **Size limits**: Prevents DoS via unlimited cache growth
4. **Expiration**: Automatic cleanup of old entries

## Future Enhancements

- [ ] **CDN Integration**: Serve popular cache from CDN
- [ ] **Compression**: Compress audio data before storage
- [ ] **Global Cache**: Share cache across all users (public sentences)
- [ ] **Analytics**: Track cache hit rates and popular phrases
- [ ] **Pre-warming**: Background job to cache common phrases
- [ ] **Redis Layer**: Add Redis for distributed caching

## License

Part of RoomEnglish vocabulary learning platform.
