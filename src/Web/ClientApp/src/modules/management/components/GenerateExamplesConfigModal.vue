<template>
  <div v-if="isOpen" class="modal-overlay" @click="closeModal">
    <div class="modal-content generate-examples-config" @click.stop>
      <div class="modal-header">
        <h2 class="modal-title">
          <Icon icon="mdi:brain" class="w-6 h-6 text-blue-500" />
          Cấu hình tạo ví dụ AI
        </h2>
        <button @click="closeModal" class="close-btn">
          <Icon icon="mdi:close" class="w-6 h-6" />
        </button>
      </div>
      
      <div class="modal-body">
        <!-- Selected Words Display -->
        <div class="selected-words-section">
          <h3 class="section-title">
            <Icon icon="mdi:format-list-bulleted" class="w-5 h-5" />
            Từ vựng đã chọn ({{ selectedWords.length }})
          </h3>
          <div class="words-container">
            <span 
              v-for="word in selectedWords" 
              :key="word"
              class="word-tag"
            >
              {{ word }}
            </span>
          </div>
        </div>

        <!-- Configuration Form -->
        <form @submit.prevent="handleGenerate" class="config-form">
          <!-- Example Count -->
          <div class="form-group">
            <label for="exampleCount" class="form-label">
              <Icon icon="mdi:counter" class="w-4 h-4" />
              Số lượng ví dụ mỗi từ
            </label>
            <div class="input-with-help">
              <input 
                id="exampleCount"
                v-model.number="config.exampleCount" 
                type="number"
                min="1"
                max="20"
                class="form-input"
                placeholder="Nhập số lượng ví dụ..."
              />
              <span class="input-help">Từ 1-20 ví dụ cho mỗi từ vựng</span>
            </div>
          </div>

          <!-- Difficulty Level -->
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:signal" class="w-4 h-4" />
              Mức độ khó (có thể chọn nhiều)
            </label>
            <div class="checkbox-group">
              <label class="checkbox-option">
                <input 
                  type="checkbox" 
                  v-model="config.difficultyLevels" 
                  :value="1"
                />
                <span class="checkbox-label">
                  <Icon icon="mdi:signal-variant" class="w-4 h-4 text-green-500" />
                  Dễ (Beginner)
                </span>
                <span class="checkbox-description">Câu đơn giản, từ vựng cơ bản</span>
              </label>
              
              <label class="checkbox-option">
                <input 
                  type="checkbox" 
                  v-model="config.difficultyLevels" 
                  :value="2"
                />
                <span class="checkbox-label">
                  <Icon icon="mdi:signal-2g" class="w-4 h-4 text-yellow-500" />
                  Trung bình (Intermediate)
                </span>
                <span class="checkbox-description">Câu dài, từ vựng phong phú hơn</span>
              </label>
              
              <label class="checkbox-option">
                <input 
                  type="checkbox" 
                  v-model="config.difficultyLevels" 
                  :value="3"
                />
                <span class="checkbox-label">
                  <Icon icon="mdi:signal-5g" class="w-4 h-4 text-red-500" />
                  Khó (Advanced)
                </span>
                <span class="checkbox-description">Câu phức tạp, ngữ pháp cao cấp</span>
              </label>
            </div>
          </div>

          <!-- Include Options -->
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:cog" class="w-4 h-4" />
              Tùy chọn bổ sung
            </label>
            <div class="checkbox-group">
              <label class="checkbox-option">
                <input 
                  type="checkbox" 
                  v-model="config.includeGrammar"
                />
                <span class="checkbox-label">
                  <Icon icon="mdi:book-open-variant" class="w-4 h-4 text-purple-500" />
                  Bao gồm giải thích ngữ pháp
                </span>
                <span class="checkbox-description">Thêm phân tích ngữ pháp cho mỗi câu</span>
              </label>
              
              <label class="checkbox-option">
                <input 
                  type="checkbox" 
                  v-model="config.includeContext"
                />
                <span class="checkbox-label">
                  <Icon icon="mdi:layers-triple" class="w-4 h-4 text-indigo-500" />
                  Đa dạng ngữ cảnh
                </span>
                <span class="checkbox-description">Tạo ví dụ trong nhiều tình huống khác nhau</span>
              </label>
            </div>
          </div>

          <!-- Estimated Time & Cost -->
          <div class="form-group">
            <div class="estimate-info">
              <div class="estimate-item">
                <Icon icon="mdi:clock-outline" class="w-5 h-5 text-blue-500" />
                <div class="estimate-content">
                  <span class="estimate-label">Thời gian ước tính:</span>
                  <span class="estimate-value">{{ estimatedTime }}</span>
                </div>
              </div>
              <div class="estimate-item">
                <Icon icon="mdi:lightning-bolt" class="w-5 h-5 text-yellow-500" />
                <div class="estimate-content">
                  <span class="estimate-label">Tổng số ví dụ:</span>
                  <span class="estimate-value">{{ totalExamples }} ví dụ</span>
                </div>
              </div>
            </div>
          </div>
        </form>
      </div>

      <div class="modal-footer">
        <button type="button" @click="closeModal" class="btn-secondary">
          <Icon icon="mdi:cancel" class="w-4 h-4" />
          Hủy bỏ
        </button>
        <button 
          type="button" 
          @click="handleGenerate" 
          class="btn-primary"
          :disabled="!isConfigValid || isGenerating"
        >
          <Icon 
            :icon="isGenerating ? 'mdi:loading' : 'mdi:brain'" 
            :class="['w-4 h-4', { 'animate-spin': isGenerating }]"
          />
          {{ isGenerating ? 'Đang tạo...' : `Tạo ${totalExamples} ví dụ` }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Icon } from '@iconify/vue'

interface GenerateConfig {
  exampleCount: number
  difficultyLevels: number[]
  includeGrammar: boolean
  includeContext: boolean
}

interface Props {
  isOpen: boolean
  selectedWords: string[]
  isGenerating?: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'generate', config: GenerateConfig): void
}

const props = withDefaults(defineProps<Props>(), {
  isGenerating: false
})

const emit = defineEmits<Emits>()

// Configuration state
const config = ref<GenerateConfig>({
  exampleCount: 5,
  difficultyLevels: [1, 2, 3],
  includeGrammar: true,
  includeContext: true
})

// Computed properties
const totalExamples = computed(() => {
  const levelsCount = config.value.difficultyLevels.length || 1
  return props.selectedWords.length * config.value.exampleCount * levelsCount
})

const estimatedTime = computed(() => {
  const baseTimePerWord = 3 // seconds per word
  const totalTime = props.selectedWords.length * baseTimePerWord
  
  if (totalTime < 60) {
    return `${totalTime}s`
  } else {
    const minutes = Math.ceil(totalTime / 60)
    return `${minutes} phút`
  }
})

const isConfigValid = computed(() => {
  return config.value.exampleCount >= 1 && 
         config.value.exampleCount <= 20 &&
         config.value.difficultyLevels.length > 0 &&
         props.selectedWords.length > 0
})

// Methods
const closeModal = () => {
  emit('close')
}

const handleGenerate = () => {
  if (!isConfigValid.value) return
  
  emit('generate', { ...config.value })
}

// Reset config when modal opens
watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    // Reset to default values when opening
    config.value = {
      exampleCount: 5,
      difficultyLevels: [1, 2, 3],
      includeGrammar: true,
      includeContext: true
    }
  }
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal-content.generate-examples-config {
  background-color: white;
  border-radius: 0.5rem;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  max-width: 42rem;
  width: 100%;
  margin: 0 1rem;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1.5rem 1.5rem 1rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.close-btn {
  color: #9ca3af;
  transition: color 0.15s ease-in-out;
}

.close-btn:hover {
  color: #4b5563;
}

.modal-body {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.selected-words-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.section-title {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.words-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.word-tag {
  padding: 0.25rem 0.75rem;
  background-color: #dbeafe;
  color: #1d4ed8;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
}

.config-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.form-label {
  display: flex;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  align-items: center;
  gap: 0.5rem;
}

.input-with-help {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  color: #1f2937;
}

.form-input {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  outline: none;
  transition: all 0.15s ease-in-out;
}

.form-input:focus {
  box-shadow: 0 0 0 2px #3b82f6;
  border-color: #3b82f6;
}

.input-help {
  font-size: 0.75rem;
  color: #6b7280;
}

.radio-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.radio-option {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.75rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: all 0.15s ease-in-out;
}

.radio-option:hover {
  background-color: #f9fafb;
}

.radio-option:has(input:checked) {
  border-color: #3b82f6;
  background-color: #eff6ff;
}

.radio-option input[type="radio"] {
  margin-top: 0.25rem;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  color: #1f2937;
}

.radio-description {
  font-size: 0.875rem;
  color: #4b5563;
  display: block;
  margin-top: 0.25rem;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.checkbox-option {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.75rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: all 0.15s ease-in-out;
}

.checkbox-option:hover {
  background-color: #f9fafb;
}

.checkbox-option:has(input:checked) {
  border-color: #10b981;
  background-color: #f0fdf4;
}

.checkbox-option input[type="checkbox"] {
  margin-top: 0.25rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  color: #1f2937;
}

.checkbox-description {
  font-size: 0.875rem;
  color: #4b5563;
  display: block;
  margin-top: 0.25rem;
}

.estimate-info {
  background-color: #f9fafb;
  border-radius: 0.5rem;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.estimate-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.estimate-content {
  display: flex;
  flex-direction: column;
}

.estimate-label {
  font-size: 0.875rem;
  color: #4b5563;
}

.estimate-value {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
}

.modal-footer {
  padding: 1rem 1.5rem 1.5rem 1.5rem;
  border-top: 1px solid #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.75rem;
}

.btn-secondary {
  padding: 0.5rem 1rem;
  color: #374151;
  background-color: #f3f4f6;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  transition: background-color 0.15s ease-in-out;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.btn-secondary:hover {
  background-color: #e5e7eb;
}

.btn-primary {
  padding: 0.5rem 1rem;
  color: white;
  background-color: #2563eb;
  border: 1px solid #2563eb;
  border-radius: 0.375rem;
  transition: background-color 0.15s ease-in-out;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.btn-primary:hover:not(:disabled) {
  background-color: #1d4ed8;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>