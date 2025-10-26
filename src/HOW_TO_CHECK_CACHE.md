# Cách Kiểm Tra Audio Cache

## 1. Kiểm tra Memory Cache (Browser RAM)

### Cách 1: Dùng Browser DevTools Console

**Bước 1**: Mở website và Browser DevTools
- Press `F12` hoặc `Ctrl+Shift+I`
- Chọn tab **Console**

**Bước 2**: Chạy lệnh JavaScript
```javascript
// Xem tất cả cache keys
console.log('Memory Cache Keys:', Array.from(window.$memoryCache?.keys() || []))

// Xem số lượng entries
console.log('Memory Cache Size:', window.$memoryCache?.size || 0)

// Xem tổng dung lượng (bytes)
const totalSize = window.$memoryCache ? 
  Array.from(window.$memoryCache.values())
    .reduce((sum, blob) => sum + blob.size, 0) : 0
console.log('Total Size (KB):', (totalSize / 1024).toFixed(2))

// Xem chi tiết từng entry
window.$memoryCache?.forEach((blob, key) => {
  console.log(`${key}: ${(blob.size / 1024).toFixed(2)} KB`)
})
```

**Bước 3**: Expose memory cache để test (thêm vào code)

Thêm vào `useSpeechSynthesis.ts`:
```typescript
// Expose for debugging
if (typeof window !== 'undefined') {
  (window as any).$memoryCache = memoryCache.value
}
```

### Cách 2: Dùng Vue DevTools

**Bước 1**: Cài đặt Vue DevTools
- Chrome: https://chrome.google.com/webstore/detail/vuejs-devtools
- Firefox: https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/

**Bước 2**: Mở Vue DevTools
- Press `F12` → Chọn tab **Vue**

**Bước 3**: Xem Composables
- Chọn component đang dùng `useSpeechSynthesis`
- Xem section **Setup** → `memoryCache`

### Cách 3: Thêm UI để hiển thị cache stats

Tạo component `CacheDebugger.vue`:
```vue
<template>
  <div class="cache-debugger">
    <button @click="showStats = !showStats">
      🔍 Cache Stats
    </button>
    
    <div v-if="showStats" class="stats-panel">
      <h3>Memory Cache (Browser RAM)</h3>
      <div class="stat">
        <span>Entries:</span>
        <strong>{{ memoryStats.count }}</strong>
      </div>
      <div class="stat">
        <span>Size:</span>
        <strong>{{ (memoryStats.size / 1024).toFixed(2) }} KB</strong>
      </div>
      
      <h4>Cached Items:</h4>
      <div v-for="(item, key) in cacheItems" :key="key" class="cache-item">
        <div class="key">{{ key }}</div>
        <div class="size">{{ (item.size / 1024).toFixed(2) }} KB</div>
      </div>
      
      <button @click="clearMemoryCache">Clear Memory Cache</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

const showStats = ref(false)
const { getMemoryCacheStats, clearMemoryCache: clearCache } = useSpeechSynthesis()

const memoryStats = computed(() => getMemoryCacheStats())

const cacheItems = computed(() => {
  const cache = (window as any).$memoryCache
  const items: Record<string, { size: number }> = {}
  
  if (cache) {
    cache.forEach((blob: Blob, key: string) => {
      items[key] = { size: blob.size }
    })
  }
  
  return items
})

const clearMemoryCache = () => {
  clearCache()
  alert('Memory cache cleared!')
}
</script>

<style scoped>
.cache-debugger {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 9999;
}

.stats-panel {
  background: white;
  border: 2px solid #333;
  border-radius: 8px;
  padding: 1rem;
  min-width: 300px;
  max-height: 500px;
  overflow-y: auto;
  box-shadow: 0 4px 12px rgba(0,0,0,0.3);
}

.stat {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid #eee;
}

.cache-item {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem;
  background: #f5f5f5;
  margin: 0.25rem 0;
  border-radius: 4px;
  font-size: 0.85rem;
}

.key {
  font-family: monospace;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  flex: 1;
  margin-right: 1rem;
}

.size {
  font-weight: bold;
  color: #007bff;
}
</style>
```

## 2. Kiểm tra Database Cache (SQL Server)

### Cách 1: Dùng SQL Server Management Studio (SSMS)

**Bước 1**: Kết nối đến database

**Bước 2**: Query tất cả cache entries
```sql
-- Xem tất cả cache entries
SELECT 
    Id,
    Text,
    Voice,
    Rate,
    Provider,
    SizeBytes,
    SizeBytes / 1024.0 AS SizeKB,
    HitCount,
    LastAccessedAt,
    ExpiresAt,
    Created
FROM AudioCaches
ORDER BY Created DESC

-- Xem cache statistics
SELECT 
    COUNT(*) AS TotalEntries,
    SUM(SizeBytes) AS TotalBytes,
    SUM(SizeBytes) / 1024.0 / 1024.0 AS TotalMB,
    AVG(SizeBytes) / 1024.0 AS AvgSizeKB,
    SUM(HitCount) AS TotalHits,
    COUNT(CASE WHEN ExpiresAt < GETUTCDATE() THEN 1 END) AS ExpiredEntries
FROM AudioCaches

-- Xem top 10 cache entries được dùng nhiều nhất
SELECT TOP 10
    Text,
    Voice,
    HitCount,
    LastAccessedAt,
    SizeBytes / 1024.0 AS SizeKB
FROM AudioCaches
ORDER BY HitCount DESC

-- Xem cache entries theo provider
SELECT 
    Provider,
    COUNT(*) AS Count,
    SUM(SizeBytes) / 1024.0 / 1024.0 AS TotalMB,
    SUM(HitCount) AS TotalHits
FROM AudioCaches
GROUP BY Provider

-- Xem cache entries sắp hết hạn
SELECT 
    Text,
    Voice,
    ExpiresAt,
    DATEDIFF(day, GETUTCDATE(), ExpiresAt) AS DaysUntilExpiry
FROM AudioCaches
WHERE ExpiresAt IS NOT NULL
ORDER BY ExpiresAt ASC
```

### Cách 2: Dùng API Endpoint

**GET /api/audio-cache/stats**

```bash
# Using curl
curl -X GET "https://localhost:5001/api/audio-cache/stats" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Using PowerShell
Invoke-RestMethod -Uri "https://localhost:5001/api/audio-cache/stats" `
  -Method GET `
  -Headers @{ "Authorization" = "Bearer YOUR_TOKEN" }
```

Response:
```json
{
  "totalEntries": 342,
  "totalSizeBytes": 17520000,
  "expiredEntries": 12,
  "oldestEntry": "2025-01-15T10:30:00Z",
  "newestEntry": "2025-10-26T14:25:00Z",
  "totalHits": 4521,
  "averageSizeKB": 51.2
}
```

### Cách 3: Tạo Admin UI để xem cache

Tạo page `AudioCacheStats.vue`:
```vue
<template>
  <div class="cache-stats-page">
    <h1>Audio Cache Statistics</h1>
    
    <div v-if="loading">Loading...</div>
    
    <div v-else-if="stats" class="stats-grid">
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalEntries }}</div>
        <div class="stat-label">Total Entries</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-value">{{ (stats.totalSizeBytes / 1024 / 1024).toFixed(2) }} MB</div>
        <div class="stat-label">Total Size</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalHits }}</div>
        <div class="stat-label">Total Hits</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-value">{{ stats.averageSizeKB.toFixed(2) }} KB</div>
        <div class="stat-label">Average Size</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-value">{{ stats.expiredEntries }}</div>
        <div class="stat-label">Expired Entries</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-value">{{ savedApiCalls }}</div>
        <div class="stat-label">API Calls Saved</div>
      </div>
    </div>
    
    <div class="actions">
      <button @click="loadStats" class="btn-refresh">
        🔄 Refresh
      </button>
      <button @click="cleanupCache" class="btn-cleanup">
        🧹 Cleanup Cache
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAudioCacheAPI } from '@/composables/useAudioCacheAPI'

const { getCacheStats, cleanupCache: cleanup } = useAudioCacheAPI()

const stats = ref<any>(null)
const loading = ref(false)

const savedApiCalls = computed(() => {
  return stats.value ? stats.value.totalHits - stats.value.totalEntries : 0
})

const loadStats = async () => {
  loading.value = true
  try {
    stats.value = await getCacheStats()
  } finally {
    loading.value = false
  }
}

const cleanupCache = async () => {
  if (confirm('Clean up expired and old cache entries?')) {
    await cleanup(100, true)
    await loadStats()
  }
}

onMounted(() => {
  loadStats()
})
</script>
```

## 3. Test Cache Flow từng bước

### Test Script trong Console

```javascript
// 1. Xem memory cache ban đầu (nên rỗng)
console.log('Initial memory cache:', window.$memoryCache?.size || 0)

// 2. Phát audio lần đầu (sẽ gọi OpenAI API)
console.time('First play')
// Click Listen button
console.timeEnd('First play') // ~1500ms

// 3. Xem memory cache sau khi phát
console.log('After first play:', window.$memoryCache?.size) // 1
window.$memoryCache?.forEach((blob, key) => {
  console.log(`Cached: ${key} (${(blob.size/1024).toFixed(2)} KB)`)
})

// 4. Phát audio lần 2 (từ memory cache)
console.time('Second play')
// Click Listen button again
console.timeEnd('Second play') // ~1ms

// 5. Reload trang và phát lại
location.reload()
// Sau khi reload...
console.log('After reload:', window.$memoryCache?.size) // 0
console.time('After reload play')
// Click Listen button
console.timeEnd('After reload play') // ~20ms (từ database)

// 6. Check memory cache đã được populate từ database
console.log('Populated from DB:', window.$memoryCache?.size) // 1
```

## 4. Network Tab - Xem API Calls

### Bước 1: Mở DevTools Network Tab
- Press `F12`
- Chọn tab **Network**

### Bước 2: Filter requests
- Filter by: `audio-cache`

### Bước 3: Test scenarios

**Scenario 1: Cache Miss (lần đầu)**
```
1. GET /api/audio-cache?text=...&voice=...  → 404 Not Found (miss)
2. POST https://api.openai.com/v1/audio/speech → 200 OK (1500ms)
3. POST /api/audio-cache → 200 OK (save to DB)
```

**Scenario 2: Database Hit (sau reload)**
```
1. GET /api/audio-cache?text=...&voice=... → 200 OK (20ms) ✅
   Response: audio/mpeg data
```

**Scenario 3: Memory Hit (same session)**
```
(Không có network request - lấy từ RAM)
```

## 5. Debugging với Logs

Thêm logs vào `useSpeechSynthesis.ts`:

```typescript
const speakWithOpenAI = async (text: string, ...) => {
  const cacheKey = `${text}_${voiceName}_${normalSpeed}`
  
  // Check memory cache
  console.log('🔍 Checking memory cache for:', cacheKey)
  let cachedBlob = memoryCache.value.get(cacheKey)
  
  if (cachedBlob) {
    console.log('✅ Memory cache HIT!', (cachedBlob.size/1024).toFixed(2), 'KB')
    return cachedBlob.arrayBuffer()
  }
  console.log('❌ Memory cache MISS')
  
  // Check database cache
  console.log('🔍 Checking database cache...')
  const dbCachedBlob = await getCachedAudio(text, voiceName, normalSpeed, provider)
  
  if (dbCachedBlob) {
    console.log('✅ Database cache HIT!', (dbCachedBlob.size/1024).toFixed(2), 'KB')
    console.log('💾 Saving to memory cache...')
    memoryCache.value.set(cacheKey, dbCachedBlob)
    return dbCachedBlob.arrayBuffer()
  }
  console.log('❌ Database cache MISS')
  
  // Call OpenAI API
  console.log('🤖 Calling OpenAI API...')
  console.time('OpenAI API call')
  const response = await tts.create(payload)
  console.timeEnd('OpenAI API call')
  
  if (response.ok) {
    const arrayBuffer = await response.arrayBuffer()
    const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' })
    console.log('✅ Got audio from OpenAI:', (blob.size/1024).toFixed(2), 'KB')
    
    console.log('💾 Saving to memory cache...')
    memoryCache.value.set(cacheKey, blob)
    
    console.log('💾 Saving to database cache...')
    saveAudioToCache(text, voiceName, normalSpeed, provider, blob, 30)
      .then(() => console.log('✅ Saved to database'))
      .catch(err => console.error('❌ Failed to save to database:', err))
    
    return arrayBuffer
  }
}
```

## 6. Performance Measurement

```javascript
// Measure cache performance
const measureCachePerformance = async (text) => {
  const iterations = 10
  const times = []
  
  for (let i = 0; i < iterations; i++) {
    const start = performance.now()
    // Trigger audio playback
    await playAudio(text)
    const end = performance.now()
    times.push(end - start)
  }
  
  console.log('Performance Results:')
  console.log('First call (API):', times[0].toFixed(2), 'ms')
  console.log('Avg cached calls:', 
    (times.slice(1).reduce((a,b) => a+b) / (iterations-1)).toFixed(2), 'ms')
  console.log('Speedup:', (times[0] / times[1]).toFixed(2) + 'x faster')
}

// Run test
measureCachePerformance('Hello world')
```

## 7. Quick Check Checklist

### Memory Cache
- [ ] Open Console
- [ ] Run: `console.log(window.$memoryCache?.size)`
- [ ] Play audio
- [ ] Run again: `console.log(window.$memoryCache?.size)` → Should increase
- [ ] Reload page
- [ ] Run: `console.log(window.$memoryCache?.size)` → Should be 0

### Database Cache
- [ ] Open Network tab
- [ ] Play new audio
- [ ] See: `POST /api/audio-cache` (saving)
- [ ] Reload page
- [ ] Play same audio
- [ ] See: `GET /api/audio-cache` → 200 OK (hit)

### SQL Server
- [ ] Open SSMS
- [ ] Run: `SELECT COUNT(*) FROM AudioCaches`
- [ ] Play audio
- [ ] Run query again → Count increased

## Tóm tắt các công cụ

| Muốn kiểm tra | Công cụ | Lệnh/Cách |
|---------------|---------|-----------|
| Memory Cache size | Console | `window.$memoryCache?.size` |
| Memory Cache keys | Console | `Array.from(window.$memoryCache?.keys())` |
| Memory Cache data | Vue DevTools | Setup → memoryCache |
| Database stats | API | `GET /api/audio-cache/stats` |
| Database entries | SSMS | `SELECT * FROM AudioCaches` |
| Network requests | DevTools Network | Filter: "audio-cache" |
| Performance | Console | `console.time()` / `console.timeEnd()` |
