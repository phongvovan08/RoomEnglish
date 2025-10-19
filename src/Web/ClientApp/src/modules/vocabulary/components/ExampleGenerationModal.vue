<template>
  <div v-if="isOpen" class="modal-overlay" @click="closeModal">
    <div class="modal-container" @click.stop>
      <div class="modal-header">
        <h3>
          <Icon icon="mdi:format-quote-close" class="w-6 h-6 mr-2" />
          Tạo ví dụ cho từ vựng "{{ vocabularyWord }}"
        </h3>
        <button @click="closeModal" class="close-button">
          <Icon icon="mdi:close" class="w-5 h-5" />
        </button>
      </div>

      <div class="modal-content">
        <div class="generation-section">
          <div class="vocab-info">
            <div class="vocab-display">
              <Icon icon="mdi:book-alphabet" class="w-8 h-8 text-blue-400" />
              <div class="vocab-details">
                <h4>{{ vocabularyWord }}</h4>
                <p>{{ vocabularyDefinition }}</p>
              </div>
            </div>
          </div>

          <div class="generation-form">
            <div class="form-group">
              <label class="input-label">
                <Icon icon="mdi:numeric" class="w-5 h-5 mr-2" />
                Số lượng ví dụ muốn tạo:
              </label>
              <input
                v-model.number="exampleCount"
                type="number"
                min="5"
                max="30"
                placeholder="Nhập số lượng (5-30)"
                class="number-input"
              />
              <div class="input-help">
                <Icon icon="mdi:information" class="w-4 h-4 mr-1" />
                <span>ChatGPT sẽ tự động tạo {{ exampleCount }} ví dụ thực tế cho từ vựng này</span>
              </div>
            </div>

            <div class="generation-options">
              <h5>Tùy chọn tạo ví dụ:</h5>
              <div class="option-group">
                <label class="checkbox-label">
                  <input v-model="includeGrammar" type="checkbox" />
                  <span class="checkmark"></span>
                  Bao gồm giải thích ngữ pháp
                </label>
                <label class="checkbox-label">
                  <input v-model="includeContext" type="checkbox" />
                  <span class="checkmark"></span>
                  Tạo ví dụ đa dạng ngữ cảnh
                </label>
                <div class="form-group">
                  <label for="difficultyLevel">Mức độ khó:</label>
                  <select v-model="difficultyLevel" id="difficultyLevel" class="form-select">
                    <option :value="1">Dễ</option>
                    <option :value="2">Trung bình</option>
                    <option :value="3">Khó</option>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <div v-if="validationErrors.length > 0" class="validation-errors">
            <div class="error-header">
              <Icon icon="mdi:alert-circle" class="w-5 h-5 mr-2" />
              Lỗi validation:
            </div>
            <ul>
              <li v-for="error in validationErrors" :key="error">{{ error }}</li>
            </ul>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button @click="closeModal" class="cancel-button">
          Hủy
        </button>
        <button 
          @click="handleGenerate" 
          :disabled="!exampleCount || exampleCount < 1 || exampleCount > 10 || isGenerating"
          class="generate-button"
        >
          <Icon 
            :icon="isGenerating ? 'mdi:loading' : 'mdi:auto-fix'" 
            :class="['w-4 h-4 mr-2', { 'animate-spin': isGenerating }]" 
          />
          {{ isGenerating ? 'Đang tạo ví dụ...' : `Tạo ${exampleCount} ví dụ` }}
        </button>
      </div>

      <!-- Success Message with Countdown -->
      <div v-if="showSuccess" class="success-overlay">
        <div class="success-message">
          <Icon icon="mdi:check-circle" class="w-8 h-8 text-green-500 mb-2" />
          <h4>Tạo ví dụ thành công!</h4>
          <p>{{ successMessage }}</p>
          <p class="countdown">Tự động đóng sau {{ countdown }}s</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Icon } from '@iconify/vue'

interface Props {
  isOpen: boolean
  vocabularyWord?: string
  vocabularyDefinition?: string
}

interface Emits {
  (e: 'close'): void
  (e: 'generation-success'): void
  (e: 'generate-examples', config: ExampleGenerationConfig): void
}

interface ExampleGenerationConfig {
  count: number
  includeGrammar: boolean
  includeContext: boolean
  difficultyLevel: number | 1
}

const props = withDefaults(defineProps<Props>(), {
  vocabularyWord: '',
  vocabularyDefinition: ''
})
const emit = defineEmits<Emits>()

const exampleCount = ref(10)
const includeGrammar = ref(true)
const includeContext = ref(true)
const difficultyLevel = ref<number | null>(1)
const validationErrors = ref<string[]>([])
const isGenerating = ref(false)
const showSuccess = ref(false)
const successMessage = ref('')
const countdown = ref(3)

const validateInput = (): boolean => {
  validationErrors.value = []
  
  if (!exampleCount.value || exampleCount.value < 1) {
    validationErrors.value.push('Số lượng ví dụ phải lớn hơn 0')
  }
  
  if (exampleCount.value > 30) {
    validationErrors.value.push('Số lượng ví dụ không được vượt quá 30')
  }
  
  return validationErrors.value.length === 0
}

const handleGenerate = async () => {
  if (!validateInput()) {
    return
  }

  isGenerating.value = true
  
  try {
    const config: ExampleGenerationConfig = {
      count: exampleCount.value,
      includeGrammar: includeGrammar.value,
      includeContext: includeContext.value,
      difficultyLevel: difficultyLevel.value
    }
    
    emit('generate-examples', config)
    // Don't reset form here - let parent handle success/error
  } catch (error) {
    console.error('Error generating examples:', error)
    isGenerating.value = false
  }
}

const closeModal = () => {
  if (!isGenerating.value) {
    emit('close')
  }
}

const startCountdown = () => {
  const timer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(timer)
      showSuccess.value = false
      emit('close')
      emit('generation-success')
    }
  }, 1000)
}

// Watch for successful generation (you would call this from parent)
const showSuccessMessage = (message: string) => {
  isGenerating.value = false
  successMessage.value = message
  showSuccess.value = true
  countdown.value = 3
  startCountdown()
}

// Handle generation error from parent
const handleGenerationError = (errorMessage?: string) => {
  isGenerating.value = false
  if (errorMessage) {
    validationErrors.value = [errorMessage]
  }
}

// Reset form when modal closes
watch(() => props.isOpen, (newValue) => {
  if (!newValue) {
    resetForm()
  }
})

const resetForm = () => {
  exampleCount.value = 10
  includeGrammar.value = true
  includeContext.value = true
  difficultyLevel.value = null
  validationErrors.value = []
  showSuccess.value = false
  isGenerating.value = false
}

// Expose methods for parent to call
defineExpose({
  showSuccessMessage,
  handleGenerationError,
  resetForm
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal-container {
  background: white;
  border-radius: 8px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  width: 100%;
  max-width: 600px;
  margin: 0 16px;
  max-height: 90vh;
  overflow: hidden;
  position: relative;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px;
  border-bottom: 1px solid #e5e7eb;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
}

.modal-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: white;
  display: flex;
  align-items: center;
}

.close-button {
  padding: 4px;
  border-radius: 9999px;
  border: none;
  background: transparent;
  color: rgba(255, 255, 255, 0.7);
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.close-button:hover {
  background-color: rgba(255, 255, 255, 0.1);
  color: white;
}

.modal-content {
  padding: 24px;
  overflow-y: auto;
  max-height: 60vh;
}

.vocab-info {
  margin-bottom: 24px;
}

.vocab-display {
  display: flex;
  align-items: center;
  padding: 16px;
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border-radius: 12px;
  border: 1px solid #bae6fd;
}

.vocab-details {
  margin-left: 12px;
}

.vocab-details h4 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #0369a1;
  margin: 0 0 4px 0;
}

.vocab-details p {
  color: #075985;
  margin: 0;
}

.generation-form {
  margin-bottom: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.input-label {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 8px;
}

.number-input {
  color: #1f2937;
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.number-input:focus {
  outline: none;
  box-shadow: 0 0 0 2px #3b82f6;
  border-color: transparent;
}

.form-select {
  color: #1f2937;
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background-color: #ffffff;
}

.form-select:focus {
  outline: none;
  box-shadow: 0 0 0 2px #3b82f6;
  border-color: transparent;
}

.input-help {
  display: flex;
  align-items: center;
  margin-top: 8px;
  padding: 8px;
  background-color: #f0f9ff;
  border-radius: 4px;
  font-size: 0.75rem;
  color: #0369a1;
}

.generation-options {
  padding: 16px;
  background-color: #f9fafb;
  border-radius: 8px;
}

.generation-options h5 {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 12px 0;
}

.option-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  color: #4b5563;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  margin-right: 8px;
  width: 16px;
  height: 16px;
}

.validation-errors {
  margin-top: 16px;
  padding: 12px;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}

.error-header {
  display: flex;
  align-items: center;
  color: #b91c1c;
  font-weight: 500;
  margin-bottom: 8px;
}

.validation-errors ul {
  list-style-type: disc;
  list-style-position: inside;
  color: #dc2626;
  font-size: 0.875rem;
}

.validation-errors li {
  margin-bottom: 4px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 24px;
  border-top: 1px solid #e5e7eb;
  background-color: #f9fafb;
}

.cancel-button {
  padding: 8px 16px;
  color: #4b5563;
  background-color: #f3f4f6;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.cancel-button:hover {
  background-color: #e5e7eb;
}

.generate-button {
  display: flex;
  align-items: center;
  padding: 8px 16px;
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
}

.generate-button:hover:not(:disabled) {
  background: linear-gradient(135deg, #059669, #047857);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
}

.generate-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.success-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
}

.success-message {
  text-align: center;
  padding: 24px;
}

.success-message h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 8px;
}

.success-message p {
  color: #4b5563;
  margin-bottom: 8px;
}

.countdown {
  font-size: 0.875rem;
  color: #6b7280;
}

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .modal-container {
    margin: 1rem;
    width: calc(100% - 2rem);
  }
  
  .vocab-display {
    flex-direction: column;
    text-align: center;
  }
  
  .vocab-details {
    margin-left: 0;
    margin-top: 8px;
  }
}
</style>