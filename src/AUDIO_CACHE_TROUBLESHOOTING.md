# Audio Cache Troubleshooting

## Error: "DEMUXER_ERROR_COULD_NOT_OPEN" or "Audio format not supported or corrupted"

This error occurs when the audio cache contains corrupted data. This typically happens when:

1. **Invalid OpenAI API key** - API returned an error message instead of audio
2. **Quota exceeded** - OpenAI returned an error response
3. **Network interruption** - Audio download was incomplete
4. **Previous bad response** - Corrupted audio was cached before validation was added

## Solution

### Quick Fix: Clear All Cache

Open browser console (F12) and run:

```javascript
// Get the speech synthesis composable
const { clearCache } = useSpeechSynthesis()

// Clear all cached audio
clearCache()

// Output: 🧹 Memory cache cleared (X entries deleted)
```

Then refresh the page and try again.

### Check Cache Contents

Before clearing, you can inspect what's cached:

```javascript
const { listCachedEntries, getCacheStats } = useSpeechSynthesis()

// View statistics
getCacheStats()
// Output:
// 📊 Audio Cache Statistics:
//    Entries: 5 / 100
//    Total Size: 0.15 MB
//    TTL: 10 minutes
//    Oldest Entry: 125 seconds ago

// List all entries
listCachedEntries()
// Output:
// 📋 Cached Audio Entries:
//    1. Hello_alloy_1.00... (4.7 KB, 45s ago)  ← SUSPICIOUS: Too small!
//    2. Goodbye_alloy_1.00... (15.2 KB, 60s ago)  ← OK
```

### Clear Specific Entry

If you know which text is corrupted:

```javascript
const { clearCacheEntry } = useSpeechSynthesis()

// Clear specific word
clearCacheEntry("Hello", "alloy", 1.0)

// Output: 🗑️ Cleared cache entry: Hello_alloy_1.00
```

### Automatic Corruption Detection

The system now automatically detects and clears corrupted cache:

1. **Size Validation**: Audio < 1KB is rejected
2. **Format Validation**: Checks for valid MP3 header (0xFF or ID3 tag)
3. **Playback Validation**: Clears cache on decode errors

When corruption is detected, you'll see:

```
⚠️ Cached memory audio has invalid MP3 format, clearing and refetching. First bytes: [73, 110, 118, 97, 108, 105, 100, 32, 65, 80]
🗑️ Clearing corrupted audio from memory cache: Hello_alloy_1.00
```

The system will automatically refetch from OpenAI API.

## Root Cause Analysis

### 1. Check OpenAI API Key

Invalid API key causes error responses to be cached as "audio":

```javascript
// Check your API key
const apiKey = import.meta.env.VITE_OPENAI_API_KEY || localStorage.getItem('openai_api_key')
console.log('API Key:', apiKey ? `${apiKey.substring(0, 10)}...${apiKey.substring(apiKey.length - 4)}` : 'NOT SET')
```

Valid OpenAI API keys start with `sk-` and are 51+ characters long.

### 2. Check OpenAI Response

When generating audio, watch console for:

```
📡 OpenAI response status: 401 Unauthorized  ← BAD: Invalid API key
📡 OpenAI response status: 429 Too Many Requests  ← BAD: Quota exceeded
📡 OpenAI response status: 200 OK  ← GOOD
📦 Received arrayBuffer: 4781 bytes  ← SUSPICIOUS if < 5000
📦 Received arrayBuffer: 15234 bytes  ← GOOD
```

### 3. Validate API Response

The system logs first 10 bytes to help diagnose:

```
🔢 First 10 bytes: [73, 110, 118, 97, 108, 105, 100, ...]  ← BAD: Text response
🔢 First 10 bytes: [73, 68, 51, 4, 0, 0, 0, ...]  ← GOOD: ID3 tag (MP3)
🔢 First 10 bytes: [255, 251, 144, 196, 0, 0, ...]  ← GOOD: MP3 frame header
```

**Valid MP3 starts with:**
- `[255, 251, ...]` - MP3 frame header (0xFF, 0xFB)
- `[73, 68, 51, ...]` - ID3 tag ("ID3")

**Invalid (error response) might start with:**
- `[123, 34, 101, ...]` - JSON error (`{"e...`)
- `[73, 110, 118, ...]` - Text error (`Inv...`)

## Prevention

### 1. Set Valid API Key

In `.env` file:

```env
VITE_OPENAI_API_KEY=sk-proj-...your-actual-key...
```

Or in browser localStorage:

```javascript
localStorage.setItem('openai_api_key', 'sk-proj-...your-actual-key...')
```

### 2. Monitor Quota

Check your OpenAI usage at: https://platform.openai.com/usage

### 3. Use Database Cache Wisely

Database cache persists corrupted audio across sessions. If you encounter errors:

1. Clear memory cache (as shown above)
2. Clear database cache via backend API:

```bash
# Call cleanup endpoint (requires authentication)
POST /audio-cache/cleanup
{
  "maxCacheSizeMB": 0,
  "deleteExpired": true
}
```

Or use SQL to clear database:

```sql
-- View corrupted entries (suspiciously small)
SELECT Id, Text, Voice, SizeBytes, CreatedAt
FROM AudioCaches
WHERE SizeBytes < 5000
ORDER BY SizeBytes;

-- Delete corrupted entries
DELETE FROM AudioCaches
WHERE SizeBytes < 5000;

-- Delete all cache
DELETE FROM AudioCaches;
```

## Updated Validation Flow

The new implementation includes 3-tier validation:

```
┌─────────────────────────────────────────┐
│ 1. Pre-Playback Validation              │
├─────────────────────────────────────────┤
│ • Check cache size > 1KB                │
│ • Check MP3 format header               │
│ • Reject invalid, fetch from API        │
└─────────────────────────────────────────┘
           ↓ (Valid cache found)
┌─────────────────────────────────────────┐
│ 2. Playback Attempt                      │
├─────────────────────────────────────────┤
│ • Create Audio element                   │
│ • Set blob URL                           │
│ • Start playback                         │
└─────────────────────────────────────────┘
           ↓ (On error)
┌─────────────────────────────────────────┐
│ 3. Post-Error Cleanup                    │
├─────────────────────────────────────────┤
│ • Detect error code 3/4 (decode error)   │
│ • Clear corrupted entry from cache       │
│ • Log error for debugging                │
│ • User can retry (will refetch)          │
└─────────────────────────────────────────┘
```

## Testing

### Test Validation

Create a fake corrupted cache entry:

```javascript
const { memoryCache, memoryCacheTimestamps } = useSpeechSynthesis()

// Create fake bad audio
const badBlob = new Blob(['Invalid audio data'], { type: 'audio/mpeg' })
const cacheKey = 'Test_alloy_1.00'

// Manually add to cache (normally not exposed)
// This will be rejected on next playback attempt
```

Then try to play "Test" - should see validation warnings and automatic refetch.

## Support

If issues persist after clearing cache:

1. Check browser console for detailed error logs
2. Verify OpenAI API key is valid
3. Check OpenAI account has available quota
4. Try different voice (some voices may have issues)
5. Test with simple text first (e.g., "Hello")

## Related Files

- `Web/ClientApp/src/composables/useSpeechSynthesis.ts` - Main TTS logic
- `Web/ClientApp/src/composables/useAudioCacheAPI.ts` - Backend cache API
- `Web/Endpoints/AudioCache.cs` - Backend endpoints
- `Application/Audio/Queries/GetAudioCacheQuery.cs` - Database cache query
