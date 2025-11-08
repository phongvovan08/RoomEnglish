<template>
  <div class="example-sidebar">
    <div class="sidebar-header">
      <div class="word-info">
        <div class="word-inline">
          <span class="word-title">{{ word.word }}</span>
          <span class="separator">|</span>
          <span class="part-of-speech">{{ word.partOfSpeech }}</span>
          <span class="separator">|</span>
          <span class="word-meaning">{{ word.meaning }}</span>
        </div>
      </div>
      <div class="progress-info">
        Group {{ groupIndex + 1 }}: {{ completedCount }}/{{ examples.length }} completed
        <span v-if="displayedCount < examples.length" class="showing-count">
          (showing {{ displayedCount }})
        </span>
      </div>
    </div>
    
    <div class="example-list" ref="listContainer">
      <div 
        v-for="(example, index) in displayedExamples" 
        :key="example.id"
        :ref="el => setItemRef(el, index)"
        class="example-item"
        :class="{ 
          'completed': isCompleted(index),
          'current': index === currentIndex 
        }"
        @click="$emit('selectExample', index)"
      >
        <div class="item-number">{{ index + 1 }}</div>
        <div class="item-content">
          <div v-if="isCompleted(index)" class="sentence-preview">
            {{ example.sentence }}
          </div>
          <div v-else class="sentence-hidden">
            *** *** ***
          </div>
        </div>
        <div class="item-status">
          <Icon 
            v-if="index === props.submittingExampleIndex" 
            icon="mdi:loading" 
            class="status-icon submitting-icon" 
          />
          <Icon 
            v-else-if="isCompleted(index)" 
            icon="mdi:check-circle" 
            class="status-icon completed-icon" 
          />
          <Icon 
            v-else-if="index === currentIndex" 
            icon="mdi:play-circle" 
            class="status-icon current-icon" 
          />
          <Icon 
            v-else 
            icon="mdi:circle-outline" 
            class="status-icon pending-icon" 
          />
        </div>
      </div>
      
      <!-- Load more trigger -->
      <div v-if="hasMore" ref="loadMoreTrigger" class="load-more-trigger">
        <Icon icon="mdi:dots-horizontal" class="w-6 h-6" />
        <span class="load-more-text">{{ loadingMore ? 'Loading...' : 'Scroll for more' }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, nextTick, onMounted, onUnmounted } from 'vue'
import { Icon } from '@iconify/vue'
import type { VocabularyExample, VocabularyWord } from '../types/vocabulary.types'

interface Props {
  examples: VocabularyExample[]
  groupIndex: number
  currentIndex: number
  completedExamples: number[] // Array of global indices
  groupStartIndex: number // Starting index of the group in all examples
  word: VocabularyWord
  submittingExampleIndex?: number // Local index of example being submitted
}

const props = defineProps<Props>()

defineEmits<{
  selectExample: [index: number]
}>()

const itemRefs = ref<(HTMLElement | null)[]>([])
const listContainer = ref<HTMLElement | null>(null)
const loadMoreTrigger = ref<HTMLElement | null>(null)

// Lazy loading state
const INITIAL_BATCH = 20 // Load 20 items initially
const LOAD_MORE_BATCH = 20 // Load 20 more items each time
const displayedCount = ref(INITIAL_BATCH)
const loadingMore = ref(false)
let observer: IntersectionObserver | null = null

const displayedExamples = computed(() => {
  return props.examples.slice(0, displayedCount.value)
})

const hasMore = computed(() => {
  return displayedCount.value < props.examples.length
})

const setItemRef = (el: any, index: number) => {
  if (el) {
    itemRefs.value[index] = el as HTMLElement
  }
}

const isCompleted = (localIndex: number) => {
  const globalIndex = props.groupStartIndex + localIndex
  return props.completedExamples.includes(globalIndex)
}

const completedCount = computed(() => {
  return props.examples.filter((_, index) => isCompleted(index)).length
})

const loadMoreExamples = () => {
  if (!hasMore.value || loadingMore.value) return
  
  loadingMore.value = true
  const newCount = Math.min(displayedCount.value + LOAD_MORE_BATCH, props.examples.length)
  
  // Simulate small delay for smooth UX
  setTimeout(() => {
    displayedCount.value = newCount
    loadingMore.value = false
    console.log(`[ExampleSidebar] Loaded ${newCount} / ${props.examples.length} examples`)
  }, 100)
}

const setupIntersectionObserver = async () => {
  await nextTick()
  
  if (!loadMoreTrigger.value || !hasMore.value) {
    return
  }
  
  if (observer) {
    observer.disconnect()
  }
  
  observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0]
      if (entry.isIntersecting && hasMore.value && !loadingMore.value) {
        loadMoreExamples()
      }
    },
    {
      root: listContainer.value,
      rootMargin: '50px',
      threshold: 0.1
    }
  )
  
  observer.observe(loadMoreTrigger.value)
}

// Watch for currentIndex changes and scroll to the current item
watch(() => props.currentIndex, async (newIndex) => {
  // If current index is beyond displayed items, load more
  if (newIndex >= displayedCount.value) {
    displayedCount.value = Math.min(newIndex + LOAD_MORE_BATCH, props.examples.length)
  }
  
  await nextTick()
  const currentItem = itemRefs.value[newIndex]
  if (currentItem) {
    currentItem.scrollIntoView({
      behavior: 'smooth',
      block: 'center'
    })
  }
}, { immediate: true })

// Watch for hasMore changes to setup/teardown observer
watch(hasMore, async (newHasMore) => {
  if (newHasMore) {
    await setupIntersectionObserver()
  } else if (observer) {
    observer.disconnect()
  }
})

// Reset displayed count when examples change
watch(() => props.examples.length, () => {
  displayedCount.value = Math.min(INITIAL_BATCH, props.examples.length)
})

onMounted(async () => {
  if (hasMore.value) {
    await setupIntersectionObserver()
  }
})

onUnmounted(() => {
  if (observer) {
    observer.disconnect()
    observer = null
  }
})
</script>

<style scoped>
.example-sidebar {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  height: fit-content;
  max-height: 80vh;
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
  position: sticky;
  top: 0;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
  z-index: 10;
  flex-shrink: 0;
}

.word-info {
  margin-bottom: 1rem;
}

.word-inline {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.word-title {
  color: white;
  font-size: 1.2rem;
  font-weight: bold;
}

.separator {
  color: rgba(255, 255, 255, 0.3);
  font-size: 1rem;
}

.word-meta {
  margin-bottom: 0.5rem;
}

.part-of-speech {
  color: #e75e8d;
  font-size: 0.9rem;
}

.word-meaning {
  color: #74c0fc;
  font-size: 1rem;
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
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.showing-count {
  color: rgba(116, 192, 252, 0.7);
  font-size: 0.85rem;
}

.example-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

.example-item {
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

.example-item:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(116, 192, 252, 0.5);
  transform: translateX(5px);
}

.example-item.current {
  border-color: #e75e8d;
  background: rgba(231, 94, 141, 0.1);
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.3);
}

.example-item.completed {
  border-color: rgba(76, 175, 80, 0.3);
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

.example-item.completed .item-number {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
}

.item-content {
  flex: 1;
  min-width: 0;
}

.sentence-preview {
  color: white;
  font-size: 0.9rem;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

.sentence-hidden {
  color: #888;
  font-size: 0.9rem;
  font-style: italic;
  letter-spacing: 2px;
}

.item-status {
  flex-shrink: 0;
}

.status-icon {
  font-size: 1.5rem;
}

.submitting-icon {
  color: #ffa726;
  animation: spin 1s linear infinite;
}

.completed-icon {
  color: #4caf50;
}

.current-icon {
  color: #e75e8d;
  animation: pulse 1.5s ease-in-out infinite;
}

.pending-icon {
  color: #666;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.load-more-trigger {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  gap: 0.5rem;
  color: rgba(116, 192, 252, 0.6);
  font-size: 0.85rem;
  min-height: 60px;
}

.load-more-text {
  font-style: italic;
}

/* Scrollbar styling */
.example-list::-webkit-scrollbar {
  width: 6px;
}

.example-list::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.example-list::-webkit-scrollbar-thumb {
  background: rgba(116, 192, 252, 0.5);
  border-radius: 3px;
}

.example-list::-webkit-scrollbar-thumb:hover {
  background: rgba(116, 192, 252, 0.7);
}

@media (max-width: 1024px) {
  .example-sidebar {
    position: static;
    max-height: none;
    margin-bottom: 1.5rem;
  }
}
</style>
