<template>
  <div class="word-sidebar">
    <div class="sidebar-header">
      <h4>📚 Vocabulary Words</h4>
      <div class="progress-info">
        {{ currentWordIndex + 1 }}/{{ words.length }} words
      </div>
    </div>
    
    <div class="word-list">
      <div 
        v-for="(word, index) in words" 
        :key="word.id"
        :ref="el => setItemRef(el, index)"
        class="word-item"
        :class="{ 
          'current': index === currentWordIndex,
          'completed': isWordCompleted(word)
        }"
        @click="$emit('selectWord', index)"
      >
        <div class="item-number">{{ index + 1 }}</div>
        <div class="item-content">
          <div class="word-text">{{ word.word }}</div>
          <div class="word-meaning">{{ word.meaning }}</div>
          <div 
            v-if="word.exampleCount > 0" 
            class="example-progress"
            :class="{ 'partial': !isWordCompleted(word) && (word.completedExampleCount || 0) >= 0 }"
          >
            {{ word.completedExampleCount || 0 }}/{{ word.exampleCount }} examples
          </div>
        </div>
        <div class="item-status">
          <Icon 
            v-if="index === currentWordIndex" 
            icon="mdi:play-circle" 
            class="status-icon current-icon" 
          />
          <Icon 
            v-else-if="isWordCompleted(word)" 
            icon="mdi:check-circle" 
            class="status-icon completed-icon" 
          />
          <Icon 
            v-else 
            icon="mdi:circle-outline" 
            class="status-icon pending-icon" 
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { Icon } from '@iconify/vue'
import type { VocabularyWord } from '../types/vocabulary.types'

interface Props {
  words: VocabularyWord[]
  currentWordIndex: number
}

const props = defineProps<Props>()

defineEmits<{
  selectWord: [index: number]
}>()

const itemRefs = ref<(HTMLElement | null)[]>([])

const setItemRef = (el: any, index: number) => {
  if (el) {
    itemRefs.value[index] = el as HTMLElement
  }
}

// Check if all examples for a word are completed
const isWordCompleted = (word: VocabularyWord): boolean => {
  if (!word.exampleCount || word.exampleCount === 0) return false
  return (word.completedExampleCount || 0) === word.exampleCount
}

// Watch for currentWordIndex changes and scroll to the current item
watch(() => props.currentWordIndex, async (newIndex) => {
  await nextTick()
  const currentItem = itemRefs.value[newIndex]
  if (currentItem) {
    currentItem.scrollIntoView({
      behavior: 'smooth',
      block: 'center'
    })
  }
}, { immediate: true })
</script>

<style scoped>
.word-sidebar {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  height: fit-content;
  max-height: 80vh;
  overflow-y: auto;
  position: sticky;
  top: 20px;
}

.sidebar-header {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-header h4 {
  color: white;
  font-size: 1.1rem;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.progress-info {
  color: #74c0fc;
  font-size: 0.9rem;
  font-weight: 500;
}

.word-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.word-item {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 0.75rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.word-item:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(116, 192, 252, 0.5);
  transform: translateX(5px);
}

.word-item.current {
  border-color: #e75e8d;
  background: rgba(231, 94, 141, 0.1);
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.3);
}

.word-item.completed {
  border-color: rgba(76, 175, 80, 0.4);
  background: rgba(76, 175, 80, 0.05);
}

.word-item.completed .item-number {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
}

.item-number {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  font-weight: bold;
  flex-shrink: 0;
}

.item-content {
  flex: 1;
  min-width: 0;
}

.word-text {
  color: white;
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.word-meaning {
  color: #74c0fc;
  font-size: 0.85rem;
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.example-progress {
  color: #4caf50;
  font-size: 0.75rem;
  margin-top: 0.25rem;
  font-weight: 500;
}

.example-progress.partial {
  color: #ffc107;
}

.item-status {
  flex-shrink: 0;
}

.status-icon {
  font-size: 1.5rem;
}

.completed-icon {
  color: #4caf50;
}

.current-icon {
  color: #e75e8d;
  animation: pulse 2s infinite;
}

.pending-icon {
  color: #888;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

/* Scrollbar styling */
.word-sidebar::-webkit-scrollbar {
  width: 6px;
}

.word-sidebar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.word-sidebar::-webkit-scrollbar-thumb {
  background: rgba(116, 192, 252, 0.5);
  border-radius: 3px;
}

.word-sidebar::-webkit-scrollbar-thumb:hover {
  background: rgba(116, 192, 252, 0.7);
}

@media (max-width: 1024px) {
  .word-sidebar {
    position: static;
    max-height: none;
    margin-bottom: 1.5rem;
  }
}
</style>
