# 📚 Vocabulary Learning Module

## 🎯 Overview

Module học từ vựng tiếng Anh với 2 chế độ học chính:
- **📚 Vocabulary Mode**: Học từ vựng (word + definition)
- **🎤 Dictation Practice**: Luyện dictation với examples

---

## 🏗️ Architecture

### **Components Structure**

```
vocabulary/
├── views/
│   ├── VocabularyLearningView.vue    # Main view - Category selection
│   └── VocabularyManagement.vue      # Admin - Manage words
│
├── components/
│   ├── LearningSession.vue           # Session controller (Word/Example tracking)
│   ├── VocabularyCard.vue            # Vocabulary learning card (Word + Definition only)
│   ├── DictationCard.vue             # Dictation practice card (Example practice)
│   ├── SessionResult.vue             # Final results display
│   └── dictation/
│       ├── word-comparison/
│       │   └── WordComparison.vue    # Real-time word comparison
│       ├── result-display/
│       │   └── ResultDisplay.vue     # Shows: English, Vietnamese, Grammar
│       └── hint-modal/
│           └── HintModal.vue         # Hint system
│
├── composables/
│   ├── useVocabulary.ts              # API calls for vocabulary
│   ├── useDictation.ts               # Dictation logic + client-side accuracy
│   └── useSpeechSettings.ts          # Global speech settings (voice, rate, pitch)
│
└── types/
    └── vocabulary.types.ts           # TypeScript interfaces
```

---

## 🎮 Learning Flow

### **Mode 1: 📚 Vocabulary**

```
Category Selection
    ↓
Learn Word 1
    • Word + Phonetic + Part of Speech
    • Definition
    • Answer questions
    ↓
Learn Word 2
    ↓
...
    ↓
Session Complete
```

**Note**: Examples đã bị loại bỏ khỏi VocabularyCard để tập trung học từ vựng.

---

### **Mode 2: 🎤 Dictation Practice**

```
Category Selection
    ↓
Word 1
    ├─ Badge: "beautiful • đẹp, xinh đẹp"
    │
    ├─ Example 1
    │   ├─ 🔊 Listen to audio (adjustable speed: 0.5x-1.25x)
    │   ├─ ⌨️ Type what you hear
    │   ├─ 📊 Real-time word-by-word comparison
    │   ├─ ✅ Submit answer
    │   └─ 📋 Result Display:
    │       • ✅/⚠️ Correct/Incorrect icon
    │       • Accuracy percentage
    │       • Your answer vs Correct answer
    │       • 🇻🇳 Vietnamese translation (flip card)
    │       • 📚 Grammar explanation
    │       • ➡️ "Next Exercise" button
    │
    ├─ Example 2
    │   └─ (Same flow)
    │
    └─ Example 3
        └─ (Same flow)
    ↓
Word 2 → Example 1 → Example 2 → ...
    ↓
...
    ↓
Session Complete
```

---

## 🎯 Key Features

### **1. DictationCard**

#### **Audio Player**
- Component: `GlobalSpeechButton`
- Features:
  - 🔊 Text-to-speech with Web Speech API
  - ⚡ Playback speed control (0.5x, 0.75x, 1x, 1.25x)
  - 🎤 Respects global speech settings (voice, pitch, rate)
  - 🚫 Bypasses browser autoplay policy (uses Web Speech API)

#### **Input Section**
- Textarea for typing
- Voice input button (Speech Recognition API)
- Timer display
- Recording indicator

#### **Word Comparison** *(Real-time)*
```typescript
States:
  ✅ Correct     → Green, ✓ icon
  ✏️ Typing      → Blue, ✏️ icon (when typing prefix correctly)
  ❌ Incorrect   → Red, ✗ icon (shows correct answer when length ≥ correct word)
  ⚠️ Missing     → Yellow, ? icon
```

**Smart Logic**:
- Typing `"hel"` for word `"hello"` → ✏️ Typing (correct prefix)
- Typing `"helo"` for word `"hello"` → Still ✏️ Typing (not enough chars)
- Typing `"helko"` for word `"hello"` → ❌ Incorrect (shows "hello")

#### **Keyboard Shortcuts**
| Key | Action | Description |
|-----|--------|-------------|
| <kbd>Ctrl</kbd> | Play Audio | Programmatically clicks Listen button |
| <kbd>Enter</kbd> | Submit | Auto-submit answer (prevents new line) |
| <kbd>Shift</kbd>+<kbd>Enter</kbd> | New Line | Normal textarea behavior |

#### **Result Display**
- Accuracy percentage with color coding
- Side-by-side comparison: Your answer vs Correct answer
- Vietnamese translation (flip card interaction)
- Grammar explanation
- Performance stats (time taken, accuracy)
- "Next Exercise" button → moves to next example

---

### **2. LearningSession**

#### **Progress Tracking**
```typescript
// For Dictation Mode
currentWordIndex      // Current word (0-based)
currentExampleIndex   // Current example in word (0-based)
currentExampleNumber  // Global example number (1-based)
totalExamples        // Total examples across all words

// Display
"Example 5 / 15"  (Dictation)
"Word 2 / 5"      (Vocabulary)
```

#### **Navigation Logic**
```typescript
nextWord() {
  if (currentExampleIndex < word.examples.length - 1) {
    // More examples in current word
    currentExampleIndex++
  } else {
    // Move to next word
    currentExampleIndex = 0
    currentWordIndex++
  }
}
```

#### **Progress Bar**
- Dictation mode: Based on examples completed
- Vocabulary mode: Based on words completed
- Real-time percentage display

---

### **3. Client-Side Accuracy Calculation**

**Location**: `useDictation.ts` → `calculateDictationAccuracy()`

```typescript
Algorithm:
1. Normalize both texts (lowercase, remove punctuation, trim)
2. Split into word arrays
3. Compare word-by-word at same position
4. Count matching words
5. Calculate percentage = (matches / max_length) * 100

Returns: DictationResult {
  isCorrect: boolean
  accuracyPercentage: number
  userInput: string
  correctAnswer: string
  timeTakenSeconds: number
  completedAt: string
}
```

**Benefits**:
- ✅ Works offline
- ✅ Instant feedback
- ✅ No API dependency
- ✅ Fire-and-forget server logging (optional)

---

## 🔧 Technical Implementation

### **Speech System Integration**

#### **Global Speech Settings**
```typescript
// useSpeechSettings.ts
- Voice selection (Web Speech API + OpenAI TTS)
- Rate (0.5 - 2.0)
- Pitch (0.5 - 2.0)
- Provider (webspeech | openai)
- Auto-initialization
```

#### **Unified Speech Synthesis**
```typescript
// useSpeechSynthesis.ts
- Instance-based management (multiple audios)
- Provider switching (Web Speech ↔ OpenAI)
- isPlaying state per instance
- Proper cleanup (stop, abort, remove listeners)
```

#### **DictationCard Speech Integration**
```typescript
// Force Web Speech API for Dictation
options.provider = 'webspeech'

Why?
- Web Speech API = Browser native, no autoplay restrictions
- OpenAI TTS = External audio, blocked by autoplay policy on keyboard events
- Keyboard shortcuts (Ctrl) need reliable audio playback
```

---

### **Component Refs Pattern**

```vue
<!-- Template -->
<GlobalSpeechButton 
  ref="listenButton"
  :text="example.sentence"
  :custom-rate="playbackSpeed"
/>

<!-- Script -->
const listenButton = ref<InstanceType<typeof GlobalSpeechButton> | null>(null)

// Programmatic click
const playAudio = () => {
  const buttonElement = listenButton.value.$el as HTMLButtonElement
  buttonElement.click()
}
```

**Benefits**:
- ✅ Consistent behavior (click vs keyboard)
- ✅ Respects button disabled state
- ✅ Uses same handlePlay() logic
- ✅ Honors customRate prop

---

## 🎨 UI/UX Features

### **Animations**
- Fade-in/scale for word badges
- Slide-in for word comparison
- Shake animation for incorrect words
- Pulse animation for typing indicator
- Keyboard hint glow on audio play

### **Visual Feedback**
- Real-time word comparison colors
- Progress bar animation
- Loading spinners
- Disabled button states
- Recording pulse indicator

### **Responsive Design**
- Mobile-optimized layouts
- Touch-friendly buttons
- Adaptive grid systems
- Keyboard hints on desktop only

---

## 📊 Data Flow

### **Submit Dictation**

```
User clicks "Submit" or presses Enter
    ↓
DictationCard.submitAnswer()
    ↓
useDictation.submitDictation(exampleId, userInput)
    ↓
calculateDictationAccuracy() [Client-side]
    ├─ Normalize texts
    ├─ Compare word-by-word
    ├─ Calculate percentage
    └─ Return DictationResult
    ↓
dictationResult.value = result
    ↓
showResult.value = true
    ↓
ResultDisplay renders with:
    • English sentence
    • Vietnamese translation
    • Grammar
    • Stats
    ↓
User clicks "Next Exercise"
    ↓
LearningSession.nextWord()
    ├─ If more examples: currentExampleIndex++
    └─ If done: currentWordIndex++, currentExampleIndex = 0
```

### **Optional Server Tracking**
```typescript
// Fire-and-forget POST to /api/vocabulary-learning/dictation/submit
fetch(...).catch(() => {
  console.log('Server tracking failed, but client result is available')
})
```

---

## 🚀 Key Improvements Made

### **1. VocabularyCard Simplification**
- ❌ Removed: Examples section (flip cards, audio buttons)
- ✅ Kept: Word, Definition, Answer questions
- 📌 Reason: Focus on vocabulary learning, examples → Dictation Practice

### **2. Dictation Practice Enhancement**
- ✅ Added: Word badge display (shows which word you're learning)
- ✅ Added: Example-by-example progression
- ✅ Added: Full result display (English, Vietnamese, Grammar)
- ✅ Added: "Next Exercise" button
- ✅ Added: Progress tracking per example

### **3. Smart Word Comparison**
- ✅ Typing state: Shows when user is typing correct prefix
- ✅ Conditional answer display: Only shows when enough chars typed
- ✅ Real-time feedback: Instant color-coded comparison

### **4. Keyboard Shortcuts**
- ✅ Ctrl = Listen (programmatic button click)
- ✅ Enter = Submit (prevents new line)
- ✅ Visual hints appear contextually

### **5. Client-Side Accuracy**
- ✅ No API dependency
- ✅ Instant results
- ✅ Works offline
- ✅ Optional server logging

---

## 🐛 Known Issues & Solutions

### **Issue: Browser Autoplay Policy**
**Problem**: OpenAI TTS audio won't play on keyboard shortcuts  
**Solution**: Force Web Speech API for Dictation (`options.provider = 'webspeech'`)

### **Issue: inconsistent Audio Playback**
**Problem**: Click button vs Ctrl had different behavior  
**Solution**: All audio playback → programmatic button click

### **Issue: Voice Index Bug**
**Problem**: voiceIndex was provider-specific, not global  
**Solution**: Fixed to use global array index in `useSpeechSettings.ts`

### **Issue: Auto-advance After Submit**
**Problem**: Result disappeared too quickly  
**Solution**: Removed auto-advance, user controls with "Next" button

---

## 📝 Future Enhancements

### **Potential Features**
- [ ] Difficulty levels (Easy/Medium/Hard examples)
- [ ] Speed reading mode
- [ ] Pronunciation practice
- [ ] Spaced repetition algorithm
- [ ] Achievement system
- [ ] Daily goals and streaks
- [ ] Export/import vocabulary lists
- [ ] Collaborative learning

### **Technical Improvements**
- [ ] Server-side accuracy calculation (when API ready)
- [ ] Offline mode (PWA)
- [ ] Audio caching
- [ ] Voice recognition accuracy tuning
- [ ] Multi-language support

---

## 🧪 Testing Checklist

### **Dictation Practice**
- [ ] Audio plays correctly with all speeds
- [ ] Voice input works (if browser supports)
- [ ] Word comparison shows real-time
- [ ] Ctrl shortcut plays audio
- [ ] Enter shortcut submits
- [ ] Result displays all info (English, Vietnamese, Grammar)
- [ ] Next button moves to next example
- [ ] Progress bar updates correctly
- [ ] Example counter shows "Example X / Y"

### **Vocabulary Mode**
- [ ] Word displays correctly
- [ ] Definition shows
- [ ] Answer questions work
- [ ] Next word button functions
- [ ] Progress bar updates

### **Settings**
- [ ] Voice selection works
- [ ] Rate adjustment applies
- [ ] Pitch adjustment applies
- [ ] Settings persist across sessions

---

## 📚 Dependencies

### **Vue 3 Composables**
- `ref`, `computed`, `onMounted`, `onUnmounted`, `watchEffect`

### **External Libraries**
- `@iconify/vue` - Icon system

### **Browser APIs**
- Web Speech API (SpeechSynthesis, SpeechRecognition)
- Fetch API
- LocalStorage

### **Custom Composables**
- `useSpeechSynthesis` - Unified TTS
- `useSpeechSettings` - Global speech settings
- `useVocabulary` - API integration
- `useDictation` - Dictation logic

---

## 👥 Developer Notes

### **Code Style**
- TypeScript strict mode
- Vue 3 Composition API
- Scoped CSS with Cyborg theme
- Component-based architecture

### **Naming Conventions**
- Components: PascalCase (`DictationCard.vue`)
- Composables: camelCase with `use` prefix (`useDictation.ts`)
- Props: camelCase
- Events: kebab-case (`@submit`, `@next`)

### **Best Practices**
- Always use TypeScript interfaces
- Add loading states for async operations
- Handle errors gracefully with user feedback
- Use computed for derived state
- Clean up listeners in `onUnmounted`
- Use refs for DOM manipulation sparingly

---

## 📞 Support

For issues or questions, contact the development team.

---

**Last Updated**: October 22, 2025  
**Version**: 2.0.0  
**Status**: ✅ Production Ready
