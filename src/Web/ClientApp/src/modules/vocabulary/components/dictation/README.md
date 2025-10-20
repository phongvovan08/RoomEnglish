# Dictation Practice Components

Các component được tách ra từ `DictationCard.vue` theo cấu trúc modular.

## 📁 Cấu trúc thư mục

```
dictation/
├── audio-player/
│   └── AudioPlayer.vue          # Điều khiển phát audio và waveform
├── input-section/
│   └── InputSection.vue         # Nhập liệu văn bản và voice input
├── result-display/
│   └── ResultDisplay.vue        # Hiển thị kết quả và so sánh
└── hint-modal/
    └── HintModal.vue            # Modal hiển thị gợi ý
```

## 🧩 Components

### 1. AudioPlayer.vue
**Mục đích**: Phát audio và hiển thị waveform animation

**Props**:
- `hasAudio: boolean` - Có audio URL hay không
- `isPlaying: boolean` - Đang phát audio
- `playbackSpeed: number` - Tốc độ phát (0.5x, 0.75x, 1x, 1.25x, 1.5x)
- `playCount: number` - Số lần đã phát

**Events**:
- `play` - Phát audio
- `change-speed` - Thay đổi tốc độ phát

**Features**:
- Nút play/pause với animation
- Điều khiển tốc độ phát
- Đếm số lần phát
- Waveform animation khi đang phát

---

### 2. InputSection.vue
**Mục đích**: Nhập liệu văn bản và voice recognition

**Props**:
- `userInput: string` - Nội dung đã nhập (v-model)
- `isRecording: boolean` - Đang ghi âm
- `elapsedTime: number` - Thời gian đã trôi qua (giây)
- `speechRecognitionSupported: boolean` - Hỗ trợ nhận diện giọng nói

**Events**:
- `update:user-input` - Cập nhật nội dung nhập
- `toggle-recording` - Bật/tắt ghi âm
- `submit` - Submit câu trả lời
- `clear` - Xóa nội dung
- `show-hint` - Hiển thị gợi ý

**Features**:
- Textarea cho nhập liệu
- Voice recognition button
- Recording indicator với pulse animation
- Timer hiển thị thời gian
- Action buttons (Submit, Clear, Hint)

---

### 3. ResultDisplay.vue
**Mục đích**: Hiển thị kết quả và phân tích

**Props**:
- `result: DictationResult` - Kết quả dictation
- `translation?: string` - Bản dịch tiếng Việt
- `grammar?: string` - Giải thích ngữ pháp

**Events**:
- `replay` - Phát lại audio
- `next` - Chuyển bài tiếp theo

**Features**:
- Result header với icon và accuracy
- So sánh câu trả lời (user vs correct)
- Flip card cho translation
- Grammar explanation section
- Performance stats (time, accuracy, trophy)
- Next button

---

### 4. HintModal.vue
**Mục đích**: Hiển thị gợi ý

**Props**:
- `show: boolean` - Hiển thị modal
- `sentence?: string` - Câu cần dictation

**Events**:
- `close` - Đóng modal

**Features**:
- Số lượng từ trong câu
- Chữ cái đầu tiên
- Gợi ý chung về pronunciation

---

## 🔄 Sử dụng trong DictationCard.vue

```vue
<template>
  <div class="dictation-card">
    <div class="card-container">
      <div class="dictation-header">
        <h2>🎤 Dictation Practice</h2>
        <div class="instruction">
          Listen to the sentence and type what you hear
        </div>
      </div>

      <AudioPlayer 
        :has-audio="!!example?.audioUrl"
        :is-playing="isPlayingAudio"
        :playback-speed="playbackSpeed"
        :play-count="playCount"
        @play="playAudio"
        @change-speed="changePlaybackSpeed"
      />

      <InputSection
        v-if="!showResult"
        v-model:user-input="userInput"
        :is-recording="isRecording"
        :elapsed-time="elapsedTime"
        :speech-recognition-supported="speechRecognitionSupported"
        @toggle-recording="toggleRecording"
        @submit="submitAnswer"
        @clear="clearInput"
        @show-hint="showHint"
      />

      <ResultDisplay
        v-if="showResult && dictationResult"
        :result="dictationResult"
        :translation="example?.translation"
        :grammar="example?.grammar"
        @replay="playCorrectAudio"
        @next="$emit('next')"
      />

      <HintModal
        :show="showHintModal"
        :sentence="example?.sentence"
        @close="closeHint"
      />
    </div>
  </div>
</template>
```

## ✨ Lợi ích của cấu trúc modular

1. **Dễ bảo trì**: Mỗi component có trách nhiệm rõ ràng
2. **Tái sử dụng**: Có thể dùng lại các component trong các tình huống khác
3. **Test**: Dễ dàng test từng component riêng lẻ
4. **Performance**: Có thể lazy load các component khi cần
5. **Clean code**: Code ngắn gọn, dễ đọc và hiểu

## 🎨 Styling

Mỗi component có scoped styles riêng, không conflict với nhau. Main card chỉ giữ lại:
- Card container styles
- Header styles
- Responsive breakpoints chung
