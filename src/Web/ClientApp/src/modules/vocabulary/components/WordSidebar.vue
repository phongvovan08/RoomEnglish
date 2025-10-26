<template>
  <div class="word-sidebar">
    <div class="sidebar-header">
      <h4>📚 Vocabulary Words</h4>
      <div class="progress-info">
        {{ currentWordIndex + 1 }}/{{ words.length }} words
      </div>
    </div>
    
    <!-- Initial Loading State -->
    <div v-if="isInitialLoading" class="initial-loading">
      <div class="loading-spinner"></div>
      <p>Preparing your learning session...</p>
    </div>
    
    <!-- Word List -->
    <RecycleScroller
      v-else
      ref="scroller"
      class="word-list"
      :items="wordsWithIndex"
      :item-size="112"
      :buffer="800"
      :page-mode="false"
      key-field="id"
      v-slot="{ item }"
      @scroll.native="handleScroll"
    >
      <div 
        class="word-item"
        :class="{ 
          'current': item.index === currentWordIndex,
          'completed': isWordCompleted(item.word)
        }"
        @click="$emit('selectWord', item.index)"
      >
        <div class="item-number">{{ item.index + 1 }}</div>
        <div class="item-content">
          <div class="word-text">{{ item.word.word }}</div>
          <div class="word-meaning">{{ item.word.meaning }}</div>
          <div 
            v-if="item.word.exampleCount > 0" 
            class="example-progress"
            :class="{ 'partial': !isWordCompleted(item.word) && (item.word.completedExampleCount || 0) >= 0 }"
          >
            {{ item.word.completedExampleCount || 0 }}/{{ item.word.exampleCount }} examples
          </div>
        </div>
        <div class="item-status">
          <Icon 
            v-if="item.index === currentWordIndex" 
            icon="mdi:play-circle" 
            class="status-icon current-icon" 
          />
          <Icon 
            v-else-if="isWordCompleted(item.word)" 
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
    </RecycleScroller>
    
    <!-- Loading indicator -->
    <div v-if="isLoadingMore" class="loading-more">
      <Icon icon="mdi:loading" class="spin-icon" />
      <span>Loading more words...</span>
    </div>
    
    <!-- End message -->
    <div v-else-if="!hasMore && words.length > 0" class="end-message">
      <Icon icon="mdi:check-circle" />
      <span>All words loaded</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'
import { RecycleScroller } from 'vue-virtual-scroller'
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css'
import { Icon } from '@iconify/vue'
import type { VocabularyWord } from '../types/vocabulary.types'

interface Props {
  words: VocabularyWord[]
  currentWordIndex: number
  hasMore?: boolean
  isLoadingMore?: boolean
  isInitialLoading?: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  selectWord: [index: number]
  loadMore: []
}>()

interface ScrollerInstance {
  scrollToItem: (index: number) => void
  scrollToPosition: (position: number) => void
}

const scroller = ref<ScrollerInstance | null>(null)
const previousWordsLength = ref(0)

// Create array with indices for virtual scrolling
const wordsWithIndex = computed(() => {
  return props.words.map((word, index) => ({
    id: word.id,
    word,
    index
  }))
})

// Check if all examples for a word are completed
const isWordCompleted = (word: VocabularyWord): boolean => {
  if (!word.exampleCount || word.exampleCount === 0) return false
  return (word.completedExampleCount || 0) === word.exampleCount
}

// Watch for currentWordIndex changes and scroll to the current item
watch(() => props.currentWordIndex, async (newIndex, oldIndex) => {
  await nextTick()
  // Only auto-scroll if user navigated (not when loading more items)
  if (scroller.value && oldIndex !== undefined && newIndex !== oldIndex) {
    const wordsLengthChanged = props.words.length !== previousWordsLength.value
    // Don't scroll if words were just loaded (length changed)
    if (!wordsLengthChanged) {
      scroller.value.scrollToItem(newIndex)
    }
  }
}, { immediate: false })

// Watch for words length changes (when loading more)
watch(() => props.words.length, (newLength, oldLength) => {
  previousWordsLength.value = newLength
  // When new words are loaded, scroll to the first new item
  if (oldLength && newLength > oldLength && scroller.value) {
    nextTick(() => {
      // Scroll to first newly loaded item
      scroller.value?.scrollToItem(oldLength)
    })
  }
})

// Handle scroll event to detect when near bottom
const handleScroll = (event: Event) => {
  const target = event.target as HTMLElement
  const scrollTop = target.scrollTop
  const scrollHeight = target.scrollHeight
  const clientHeight = target.clientHeight
  
  // If scrolled to within 200px of bottom and has more items
  if (scrollHeight - scrollTop - clientHeight < 200 && props.hasMore && !props.isLoadingMore) {
    console.log('Near bottom, loading more words...')
    emit('loadMore')
  }
}

</script>

<style scoped>
.word-sidebar {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  height: 80vh;
  overflow: hidden;
  position: sticky;
  top: 20px;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  flex-shrink: 0;
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
  flex: 1;
  overflow-y: auto;
  height: 100%;
  min-height: 400px;
}

/* Override vue-virtual-scroller styles */
.word-list :deep(.vue-recycle-scroller__item-wrapper) {
  padding: 0 4px;
}

.word-list :deep(.vue-recycle-scroller__item-view) {
  padding-bottom: 0.75rem;
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
  height: 100px;
  box-sizing: border-box;
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

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.loading-more,
.end-message {
  padding: 1rem;
  text-align: center;
  color: #74c0fc;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.spin-icon {
  animation: spin 1s linear infinite;
  font-size: 1.2rem;
}

.end-message {
  color: #4caf50;
}

.initial-loading {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 2rem;
}

.initial-loading p {
  color: #74c0fc;
  font-size: 0.95rem;
  margin: 0;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 3px solid rgba(116, 192, 252, 0.2);
  border-top-color: #74c0fc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* Scrollbar styling for virtual scroller */
.word-list :deep(.vue-recycle-scroller__item-wrapper)::-webkit-scrollbar {
  width: 6px;
}

.word-list :deep(.vue-recycle-scroller__item-wrapper)::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.word-list :deep(.vue-recycle-scroller__item-wrapper)::-webkit-scrollbar-thumb {
  background: rgba(116, 192, 252, 0.5);
  border-radius: 3px;
}

.word-list :deep(.vue-recycle-scroller__item-wrapper)::-webkit-scrollbar-thumb:hover {
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
