# 🔊 Hướng dẫn tích hợp OpenAI TTS (ChatGPT) 

## 📋 Tổng quan

Hệ thống TTS hiện tại hỗ trợ 2 providers:

1. **🤖 OpenAI TTS (ChatGPT)** - Chất lượng cao nhất, cần API key
2. **🗣️ Web Speech API** - Miễn phí, sử dụng giọng của trình duyệt

## 🚀 Cách sử dụng OpenAI TTS

### Bước 1: Lấy API Key từ OpenAI

1. Truy cập [platform.openai.com](https://platform.openai.com)
2. Đăng ký/đăng nhập tài khoản
3. Vào **API Keys** → **Create new secret key**
4. Copy key (dạng: `sk-...`)

### Bước 2: Cấu hình trong ứng dụng

#### Option 1: Qua Settings Panel
1. Mở Settings (⚙️) trong vocabulary card
2. Chọn **OpenAI TTS (ChatGPT)**
3. Nhập API key vào ô **OpenAI API Key**
4. Click **Save**

#### Option 2: Qua Test Page
1. Vào `/test-tts`
2. Chọn **OpenAI TTS (ChatGPT)** trong dropdown
3. Nhập API key
4. Click **Save**

#### Option 3: Environment Variable
Thêm vào file `.env`:
```
VITE_OPENAI_API_KEY=sk-your-api-key-here
```

### Bước 3: Chọn giọng đọc

OpenAI TTS có 6 giọng chất lượng cao:

- **Alloy** - Neutral, balanced
- **Echo** - Male, clear
- **Fable** - British, expressive  
- **Nova** - Female, warm
- **Onyx** - Male, deep
- **Shimmer** - Female, soft

## 💰 Chi phí

OpenAI TTS có giá **$15/1M characters** (~$0.015/1000 từ)

Ví dụ: 1000 từ vocabulary ≈ $0.015

## ⚡ Tính năng

### Ưu điểm OpenAI TTS:
- ✅ Chất lượng giọng nói tự nhiên nhất
- ✅ Hỗ trợ nhiều ngôn ngữ 
- ✅ Ổn định, không phụ thuộc trình duyệt
- ✅ Tốc độ phản hồi nhanh

### Auto-fallback:
- Nếu thiếu API key → Chuyển sang Web Speech API
- Nếu OpenAI TTS lỗi → Chuyển sang Web Speech API
- Đảm bảo TTS luôn hoạt động

## 🔧 Cấu hình nâng cao

### Trong code:

```typescript
// Sử dụng OpenAI TTS với custom options
await speak(text, instanceId, {
  provider: 'openai',
  voiceIndex: 0, // Alloy voice
  rate: 0.8,
  pitch: 1.0
})
```

### Environment variables:
```bash
# API Key
VITE_OPENAI_API_KEY=sk-...

# Custom model (optional)
VITE_OPENAI_TTS_MODEL=tts-1-hd
```

## 🚨 Lưu ý bảo mật

- ⚠️ **Không commit API key** vào Git
- ✅ Sử dụng environment variables
- ✅ API key được lưu trong localStorage (chỉ local)
- ✅ Key không được gửi qua network (chỉ dùng client-side)

## 🧪 Testing

1. Vào `/test-tts`
2. Chọn OpenAI provider  
3. Nhập API key
4. Test với văn bản mẫu
5. So sánh chất lượng giữa các providers

## 🎯 Kết quả

Sau khi cấu hình xong:

- Học vocabulary với giọng ChatGPT chất lượng cao
- Tự động fallback nếu có lỗi
- Settings được lưu tự động
- Hỗ trợ tất cả tính năng speed/pitch control

---

**Lưu ý**: OpenAI TTS có thể mất 1-2 giây để generate audio lần đầu, sau đó sẽ nhanh hơn nhờ caching.