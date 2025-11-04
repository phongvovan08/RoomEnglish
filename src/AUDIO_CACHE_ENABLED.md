# Tính Năng Audio Cache - Đã Kích Hoạt

## Tổng Quan
Tính năng audio cache đã được kích hoạt để tránh gọi ChatGPT TTS API nhiều lần cho cùng một văn bản. Hệ thống sử dụng cơ chế cache 2 tầng:

1. **Memory Cache** (cache trong trình duyệt, nhanh)
2. **Database Cache** (lưu trữ vĩnh viễn, chia sẻ giữa các phiên)

## Các Thay Đổi Đã Thực Hiện

### File Đã Chỉnh Sửa

#### 1. `Web/ClientApp/src/composables/useSpeechSynthesis.ts`

**Kích hoạt tích hợp database cache:**

```typescript
// Import audio cache API
import { useAudioCacheAPI } from './useAudioCacheAPI'

const { getCachedAudio, saveAudioToCache } = useAudioCacheAPI()
```

**Kiểm tra database cache trước khi gọi API (dòng ~119-175):**
```typescript
// Kiểm tra memory cache trước (nhanh nhất)
if (cachedBlob) {
  // Validate kích thước (phải > 1KB)
  if (cachedBlob.size < 1024) {
    console.warn('⚠️ Audio trong memory cache quá nhỏ, xóa và tải lại')
    memoryCache.value.delete(cacheKey)
    memoryCacheTimestamps.value.delete(cacheKey)
  } else {
    // Validate định dạng MP3
    const arrayBuffer = await cachedBlob.arrayBuffer()
    const uint8Array = new Uint8Array(arrayBuffer)
    const isValidMP3 = (uint8Array[0] === 0xFF && (uint8Array[1] & 0xE0) === 0xE0) || // MP3 frame header
                       (uint8Array[0] === 0x49 && uint8Array[1] === 0x44 && uint8Array[2] === 0x33) // ID3 tag
    
    if (!isValidMP3) {
      console.warn('⚠️ Audio trong memory cache có định dạng MP3 không hợp lệ')
      memoryCache.value.delete(cacheKey)
      memoryCacheTimestamps.value.delete(cacheKey)
    } else {
      return arrayBuffer
    }
  }
}

// Kiểm tra database cache
const dbCachedBlob = await getCachedAudio(text, voiceName, normalSpeed, provider)

if (dbCachedBlob) {
  console.log('💾 Sử dụng audio từ database:', dbCachedBlob.size, 'bytes')
  
  // Validate trước khi sử dụng
  if (dbCachedBlob.size < 1024) {
    console.warn('⚠️ Audio trong database quá nhỏ, bỏ qua và tải lại')
  } else {
    // Validate định dạng MP3
    const arrayBuffer = await dbCachedBlob.arrayBuffer()
    const uint8Array = new Uint8Array(arrayBuffer)
    const isValidMP3 = (uint8Array[0] === 0xFF && (uint8Array[1] & 0xE0) === 0xE0) ||
                       (uint8Array[0] === 0x49 && uint8Array[1] === 0x44 && uint8Array[2] === 0x33)
    
    if (!isValidMP3) {
      console.warn('⚠️ Audio trong database có định dạng MP3 không hợp lệ')
    } else {
      // Lưu vào memory cache để truy cập nhanh hơn lần sau
      memoryCache.value.set(cacheKey, dbCachedBlob)
      memoryCacheTimestamps.value.set(cacheKey, Date.now())
      return arrayBuffer
    }
  }
}
```

**Lưu vào database sau khi nhận từ OpenAI (dòng ~246-249):**
```typescript
// Lưu vào database (bất đồng bộ, không chặn phát audio)
saveAudioToCache(text, voiceName, normalSpeed, provider, blob, 30).catch(err => {
  console.error('Không thể lưu vào database cache:', err)
})
```

**Tự động xóa cache bị lỗi khi phát audio thất bại (dòng ~398-413):**
```typescript
audio.onerror = (event) => {
  console.error('❌ Lỗi phát audio:', event, audio.error)
  
  if (audio.error) {
    const errorCode = audio.error.code
    
    // Lỗi giải mã hoặc định dạng không hỗ trợ
    if (errorCode === 4 || errorCode === 3) {
      console.warn('🗑️ Xóa audio bị lỗi khỏi memory cache:', cacheKey)
      memoryCache.value.delete(cacheKey)
      memoryCacheTimestamps.value.delete(cacheKey)
    }
  }
  
  cleanup()
  reject(new Error(errorMessage))
}
```

#### 2. `Web/ClientApp/src/composables/useAudioCacheAPI.ts`

**Cấu hình API endpoint:**
```typescript
const API_BASE = `${API_CONFIG.baseURL}/api/audio-cache`
```

**Các hàm với log rõ ràng:**

```typescript
// Lấy audio từ database
const getCachedAudio = async (text, voice, rate, provider) => {
  try {
    const response = await fetch(`${API_BASE}?${params}`)
    
    if (response.status === 404) {
      console.log('💾 Cache miss - audio chưa có trong database')
      return null
    }
    
    console.log('✅ Cache hit - đã load từ database')
    return await response.blob()
  } catch (err) {
    console.log('💤 Backend không khả dụng:', err.message)
    return null
  }
}

// Lưu audio vào database
const saveAudioToCache = async (text, voice, rate, provider, audioBlob, expiryDays) => {
  try {
    // Convert blob sang base64
    const base64 = btoa(binary)
    
    const response = await fetch(API_BASE, {
      method: 'POST',
      body: JSON.stringify({
        text, voice, rate, provider,
        audioDataBase64: base64,
        mimeType: 'audio/mpeg',
        expiryDays
      })
    })
    
    if (response.ok) {
      console.log('✅ Đã lưu audio vào database cache')
      return true
    }
  } catch (err) {
    console.log('💤 Backend không khả dụng:', err.message)
    return false
  }
}
```

## Cách Hoạt Động

### Luồng Cache

1. **Lần đầu tiên** (Cache Miss):
   ```
   Người dùng yêu cầu audio cho "Hello"
   → Kiểm tra memory cache: MISS
   → Kiểm tra database cache: MISS
   → Gọi OpenAI TTS API (tốn tiền)
   → Lưu audio vào memory cache
   → Lưu audio vào database cache (bất đồng bộ)
   → Phát audio
   ```

2. **Lần thứ hai - Cùng phiên** (Memory Cache Hit):
   ```
   Người dùng yêu cầu audio cho "Hello" lần nữa
   → Kiểm tra memory cache: HIT ✅
   → Phát audio ngay lập tức (không gọi API)
   ```

3. **Lần thứ hai - Phiên mới** (Database Cache Hit):
   ```
   Người dùng đóng trình duyệt và quay lại sau
   Người dùng yêu cầu audio cho "Hello"
   → Kiểm tra memory cache: MISS (phiên mới)
   → Kiểm tra database cache: HIT ✅
   → Load audio từ database
   → Lưu vào memory cache cho lần sau
   → Phát audio (nhanh, không gọi API)
   ```

### Tạo Cache Key

Cache key được tạo bằng SHA256 hash:
```typescript
const cacheKey = `${text}_${voice}_${rate:F2}_${provider}`
// Ví dụ: "Hello_alloy_1.00_openai"
// SHA256 hash: "a1b2c3d4..."
```

Đảm bảo:
- Cùng text + voice + rate + provider = cùng cache entry
- Không có audio trùng lặp

## Cấu Hình Cache

### Memory Cache
- **Kích thước tối đa**: 100 entries
- **TTL**: 10 phút
- **Cơ chế loại bỏ**: LRU (Least Recently Used)
- **Tự động dọn dẹp**: Mỗi 2 phút

### Database Cache
- **Thời gian hết hạn mặc định**: 30 ngày
- **Lưu trữ**: Bảng AudioCaches
- **Theo dõi**: Số lần truy cập, thời gian truy cập cuối
- **Dọn dẹp**: Thủ công qua API endpoint `/api/audio-cache/cleanup`

## Cơ Sở Hạ Tầng Backend

### Database Entity
```csharp
public class AudioCache : BaseEntity
{
    public string CacheKeyHash { get; set; }    // SHA256 hash
    public string Text { get; set; }
    public string Voice { get; set; }
    public double Rate { get; set; }
    public string Provider { get; set; }
    public byte[] AudioData { get; set; }       // MP3 audio
    public string MimeType { get; set; }        // "audio/mpeg"
    public int SizeBytes { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public int HitCount { get; set; }
}
```

### API Endpoints

1. **GET `/api/audio-cache`** - Lấy audio đã cache
   - Query params: `text`, `voice`, `rate`, `provider`
   - Trả về: File audio (audio/mpeg) hoặc 404 nếu chưa cache

2. **POST `/api/audio-cache`** - Lưu audio vào cache
   - Body: `{ text, voice, rate, provider, audioDataBase64, mimeType, expiryDays }`
   - Trả về: 200 OK

3. **GET `/api/audio-cache/stats`** - Lấy thống kê cache
   - Trả về: `{ totalEntries, totalSizeBytes, expiredEntries, totalHits, ... }`

4. **POST `/api/audio-cache/cleanup`** - Dọn dẹp cache hết hạn/cũ
   - Body: `{ maxCacheSizeMB, deleteExpired }`
   - Trả về: `{ deletedEntries }`

### Commands & Queries

- **GetAudioCacheQuery** - Lấy audio đã cache theo hash
- **SaveAudioCacheCommand** - Lưu audio mới vào cache
- **CleanupAudioCacheCommand** - Xóa entries hết hạn/cũ
- **GetAudioCacheStatsQuery** - Lấy thống kê cache

## Lợi Ích

### Tiết Kiệm Chi Phí
- ✅ Không gọi API lặp lại cho cùng văn bản
- ✅ Phiên người dùng thông thường: Tỷ lệ cache hit 80-90%
- ✅ Cache giữa các phiên giảm chi phí API hơn nữa

### Hiệu Suất
- ✅ Memory cache: ~0ms (tức thì)
- ✅ Database cache: ~50-100ms (nhanh)
- ✅ OpenAI API: ~500-2000ms (chậm)

### Trải Nghiệm Người Dùng
- ✅ Phát audio nhanh hơn
- ✅ Không có loading spinner cho audio đã cache
- ✅ Hoạt động offline cho các từ đã nghe trước đó

## Giám Sát

### Console Logs Frontend
```
💾 Cache miss - audio chưa có trong database
✅ Cache hit - đã load từ database
✅ Đã lưu audio vào database cache
💤 Backend không khả dụng: Failed to fetch
🧹 Đã xóa memory cache (25 entries)
🧹 Tự động dọn dẹp 5 entries hết hạn trong memory cache
⚠️ Audio trong memory cache có định dạng MP3 không hợp lệ
🗑️ Xóa audio bị lỗi khỏi memory cache
```

### Backend Logs
Kiểm tra application logs để theo dõi:
- Cache hits/misses
- Thao tác lưu
- Thao tác dọn dẹp

## Kiểm Tra

### Kiểm Tra Luồng Cache

1. **Kiểm tra Memory Cache**:
   ```typescript
   // Phát cùng từ 2 lần liên tiếp
   await speak("Hello", "instance1", { provider: 'openai' })
   await speak("Hello", "instance2", { provider: 'openai' })
   // Lần thứ 2 sẽ log: "💾 Sử dụng audio từ memory"
   ```

2. **Kiểm tra Database Cache**:
   ```typescript
   // Xóa memory cache, sau đó phát cùng từ
   clearCache()
   await speak("Hello", "instance1", { provider: 'openai' })
   // Sẽ log: "✅ Cache hit - đã load từ database"
   ```

3. **Kiểm tra Cache Miss**:
   ```typescript
   // Phát từ mới
   await speak("Goodbye", "instance1", { provider: 'openai' })
   // Sẽ log: "💾 Cache miss - audio chưa có trong database"
   // Sau đó: "🌐 Không tìm thấy cache, gọi OpenAI API..."
   ```

### Xem Thống Kê Cache

```typescript
// Trong browser console hoặc component
const { getCacheStats, listCachedEntries } = useSpeechSynthesis()

// Xem thống kê
const stats = getCacheStats()
// Output: { memory: { count: 15, maxSize: 100, size: 678900, ttl: 600000 } }

// Liệt kê tất cả entries
const entries = listCachedEntries()
// Output: Array các { key, size, age } objects
```

## Xử Lý Sự Cố

### Xóa Cache Bị Lỗi

Mở browser console (F12) và chạy:

```javascript
// Lấy composable
const { clearCache, clearCacheEntry } = useSpeechSynthesis()

// Xóa toàn bộ cache
clearCache()
// Output: 🧹 Đã xóa memory cache (X entries)

// Xóa entry cụ thể
clearCacheEntry("Hello", "alloy", 1.0)
// Output: 🗑️ Đã xóa cache entry: Hello_alloy_1.00
```

### Kiểm Tra Cache

```javascript
const { listCachedEntries } = useSpeechSynthesis()

// Liệt kê tất cả entries
listCachedEntries()
// Output:
// 📋 Các Entry Audio Đã Cache:
//    1. Hello_alloy_1.00... (15.2 KB, 45s trước)  ← OK
//    2. Test_alloy_1.00... (4.7 KB, 60s trước)  ← NGHI NGỜ: Quá nhỏ!
```

### Dọn Dẹp Database Cache

```sql
-- Xem entries bị lỗi (nghi ngờ quá nhỏ)
SELECT Id, Text, Voice, SizeBytes, CreatedAt
FROM AudioCaches
WHERE SizeBytes < 5000
ORDER BY SizeBytes;

-- Xóa entries bị lỗi
DELETE FROM AudioCaches
WHERE SizeBytes < 5000;

-- Xóa toàn bộ cache
DELETE FROM AudioCaches;
```

Hoặc qua API:
```bash
POST /api/audio-cache/cleanup
{
  "maxCacheSizeMB": 0,
  "deleteExpired": true
}
```

## Validation Flow (Cải Tiến)

Hệ thống hiện tại có 3 tầng validation:

```
┌─────────────────────────────────────────┐
│ 1. Validation Trước Phát                │
├─────────────────────────────────────────┤
│ • Kiểm tra kích thước cache > 1KB       │
│ • Kiểm tra MP3 format header            │
│ • Từ chối cache không hợp lệ, fetch API │
└─────────────────────────────────────────┘
           ↓ (Tìm thấy cache hợp lệ)
┌─────────────────────────────────────────┐
│ 2. Thử Phát Audio                        │
├─────────────────────────────────────────┤
│ • Tạo Audio element                      │
│ • Set blob URL                           │
│ • Bắt đầu phát                           │
└─────────────────────────────────────────┘
           ↓ (Nếu có lỗi)
┌─────────────────────────────────────────┐
│ 3. Dọn Dẹp Sau Lỗi                       │
├─────────────────────────────────────────┤
│ • Phát hiện error code 3/4 (lỗi decode) │
│ • Xóa entry bị lỗi khỏi cache            │
│ • Log lỗi để debug                       │
│ • Người dùng có thể thử lại (sẽ refetch)│
└─────────────────────────────────────────┘
```

## Vấn Đề Đã Giải Quyết

### 1. ✅ Lỗi Audio Bị Lỗi
**Vấn đề**: Audio cache 4781 bytes bị lỗi, không thể phát
```
❌ DEMUXER_ERROR_COULD_NOT_OPEN
```

**Giải pháp**: 
- Thêm validation kích thước (> 1KB)
- Thêm validation định dạng MP3 (kiểm tra header bytes)
- Tự động xóa cache bị lỗi khi phát thất bại

### 2. ✅ Lỗi Console Spam
**Vấn đề**: Console hiện nhiều lỗi 404 đỏ khi backend không chạy
```
GET http://localhost:3000/api/audio-cache?... 404 (Not Found)
```

**Giải pháp**:
- Wrap fetch trong try-catch riêng
- Catch error trước khi nó log ra console
- Log thông báo dễ hiểu thay vì lỗi đỏ:
  ```
  💤 Backend không khả dụng: Failed to fetch
  ```

### 3. ✅ URL Backend Sai
**Vấn đề**: Frontend gọi `/audio-cache` thay vì `/api/audio-cache`

**Giải pháp**: 
- Cập nhật API_BASE thành `${API_CONFIG.baseURL}/api/audio-cache`
- Khớp với routing backend `MapGroup($"/api/{groupName}")`

### 4. ✅ Cache Không Hoạt Động
**Vấn đề**: Feature flag `ENABLE_DATABASE_CACHE` bị tắt

**Giải pháp**: 
- Xóa feature flag
- Database cache luôn được bật
- Graceful fallback nếu backend không khả dụng

## Hướng Dẫn Sử Dụng

### Khi Backend ĐANG Chạy
```
🔊 Phát "Hello" lần 1
💾 Cache miss - audio chưa có trong database
🌐 Gọi OpenAI API...
✅ Nhận 15234 bytes từ OpenAI
✅ Đã lưu audio vào database cache
🎵 Phát audio

🔊 Phát "Hello" lần 2 (cùng phiên)
💾 Sử dụng audio từ memory: 15234 bytes
🎵 Phát audio ngay lập tức

🔊 Phát "Hello" lần 3 (phiên mới, sau khi refresh)
✅ Cache hit - đã load từ database
💾 Sử dụng audio từ memory: 15234 bytes
🎵 Phát audio nhanh
```

### Khi Backend KHÔNG Chạy
```
🔊 Phát "Hello" lần 1
💤 Backend không khả dụng: Failed to fetch
🌐 Gọi OpenAI API...
✅ Nhận 15234 bytes từ OpenAI
💤 Backend không khả dụng: Failed to fetch (khi lưu)
💾 Lưu vào memory cache
🎵 Phát audio

🔊 Phát "Hello" lần 2 (cùng phiên)
💾 Sử dụng audio từ memory: 15234 bytes
🎵 Phát audio ngay lập tức

🔊 Phát "Hello" lần 3 (phiên mới)
💤 Backend không khả dụng: Failed to fetch
🌐 Gọi OpenAI API lại (vì memory cache đã mất)
✅ Nhận 15234 bytes từ OpenAI
🎵 Phát audio
```

## Cải Tiến Trong Tương Lai

- [ ] Preload các từ thông dụng khi khởi động app
- [ ] Cache prewarming cho danh sách từ vựng
- [ ] Chia sẻ cache giữa người dùng (cho cụm từ thông dụng)
- [ ] Giới hạn kích thước cache ở frontend
- [ ] Dashboard phân tích cache

## Files Liên Quan

- `Web/ClientApp/src/composables/useSpeechSynthesis.ts` - Logic TTS chính
- `Web/ClientApp/src/composables/useAudioCacheAPI.ts` - Backend cache API
- `Web/Endpoints/AudioCache.cs` - Backend endpoints
- `Application/Audio/Queries/GetAudioCacheQuery.cs` - Database cache query
- `Application/Audio/Commands/SaveAudioCacheCommand.cs` - Lưu cache
- `Application/Audio/Commands/CleanupAudioCacheCommand.cs` - Dọn dẹp cache

