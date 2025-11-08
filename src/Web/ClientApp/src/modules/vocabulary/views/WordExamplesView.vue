<template>
  <div class="word-examples-view">
    <div class="breadcrumb">
      <router-link :to="{ name: 'VocabularyLearningCategories' }">
        Categories
      </router-link>
      <span v-if="word"> / </span>
      <router-link 
        v-if="word && word.categoryId"
        :to="{ name: 'VocabularyLearningWords', query: { categoryId: word.categoryId } }"
      >
        {{ word.categoryName || 'Words' }}
      </router-link>
      <span v-if="word"> / {{ word.word }}</span>
    </div>

    <div class="view-header">
      <div class="word-info">
        <h1>{{ word?.word }}</h1>
        <span class="phonetic" v-if="word?.phonetic">{{ word.phonetic }}</span>
      </div>
      <p class="meaning" v-if="word">{{ word.meaning }}</p>
      <p class="vietnamese" v-if="word?.vietnameseMeaning">{{ word.vietnameseMeaning }}</p>
    </div>

    <div v-if="loading" class="loading">
      <Icon icon="mdi:loading" class="animate-spin w-8 h-8" />
      Loading examples...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <div v-else class="examples-section">
      <div class="section-header">
        <h2>Practice Examples ({{ displayedExamples.length }} of {{ allExamples.length }})</h2>
        <button 
          v-if="hasMore" 
          @click="loadMoreExamples" 
          class="load-more-btn"
          :disabled="loadingMore"
        >
          <Icon v-if="loadingMore" icon="mdi:loading" class="animate-spin" />
          <span v-else>Load More</span>
        </button>
      </div>
      <div class="examples-grid">
        <DictationCard
          v-for="(example, index) in displayedExamples"
          :key="example.id"
          :example="example"
          :index="index"
          @next="handleNext(index)"
        />
      </div>
      
      <!-- Load more trigger - always render when hasMore -->
      <div 
        v-if="hasMore" 
        ref="loadMoreTrigger" 
        class="load-more-trigger"
      >
        <Icon icon="mdi:loading" class="animate-spin w-6 h-6" />
        <span v-if="loadingMore">Loading more examples...</span>
        <span v-else>Scroll to load more...</span>
      </div>
      
      <div v-if="!hasMore && allExamples.length > 0" class="all-loaded">
        <Icon icon="mdi:check-circle" class="w-6 h-6" />
        <span>All {{ allExamples.length }} examples loaded</span>
      </div>
    </div>

    <div v-if="!loading && allExamples.length === 0" class="empty-state">
      <Icon icon="mdi:text-box-outline" class="w-16 h-16 opacity-50" />
      <p>No examples found for this word</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import { useVocabularyWords } from '../composables/use-vocabulary-words'
import DictationCard from '../components/DictationCard.vue'
import type { DictationResult } from '../types/vocabulary.types'

const route = useRoute()
const router = useRouter()

const { loadWordById, currentWord } = useVocabularyWords()

const wordId = ref<number>(Number(route.query.wordId))
const word = ref<any>(null)
const allExamples = ref<any[]>([])
const displayedExamples = ref<any[]>([])
const loading = ref(false)
const loadingMore = ref(false)
const error = ref<string | null>(null)
const loadMoreTrigger = ref<HTMLElement | null>(null)

const BATCH_SIZE = 10 // Load 10 examples at a time
let observer: IntersectionObserver | null = null

const hasMore = computed(() => displayedExamples.value.length < allExamples.value.length)

const loadData = async () => {
  loading.value = true
  error.value = null
  
  try {
    console.log('[WordExamplesView] Loading word:', wordId.value)
    
    // Load word with examples
    await loadWordById(wordId.value)
    word.value = currentWord.value
    
    console.log('[WordExamplesView] Word loaded:', word.value)
    
    if (word.value && word.value.examples) {
      allExamples.value = word.value.examples
      // Initially load first batch
      loadMoreExamples()
    }
    
    console.log('[WordExamplesView] Total examples:', allExamples.value.length)
  } catch (err: any) {
    console.error('Failed to load word examples:', err)
    error.value = err.message || 'Failed to load examples'
  } finally {
    loading.value = false
  }
}

const loadMoreExamples = () => {
  if (!hasMore.value) return
  
  const currentLength = displayedExamples.value.length
  const nextBatch = allExamples.value.slice(currentLength, currentLength + BATCH_SIZE)
  displayedExamples.value.push(...nextBatch)
  
  console.log('[WordExamplesView] Loaded batch:', nextBatch.length, 'examples. Total displayed:', displayedExamples.value.length)
}

const setupIntersectionObserver = async () => {
  await nextTick()

  if (!loadMoreTrigger.value) {
    console.log('[WordExamplesView] Load trigger element not found')
    return
  }

  console.log('[WordExamplesView] Setting up intersection observer')
  
  observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0]
      console.log('[WordExamplesView] Observer callback - isIntersecting:', entry.isIntersecting, 'hasMore:', hasMore.value, 'loadingMore:', loadingMore.value)
      
      if (entry.isIntersecting && hasMore.value && !loadingMore.value) {
        console.log('[WordExamplesView] ✅ Loading more examples...')
        loadingMore.value = true
        
        // Use setTimeout to prevent blocking
        setTimeout(() => {
          loadMoreExamples()
          loadingMore.value = false
        }, 100)
      }
    },
    {
      root: null,
      rootMargin: '200px',
      threshold: 0.1
    }
  )

  observer.observe(loadMoreTrigger.value)
  console.log('[WordExamplesView] ✅ Observer attached')
}

const handleNext = (currentIndex: number) => {
  console.log('Next example after:', currentIndex)
}

onMounted(async () => {
  console.log('[WordExamplesView] Component mounted')
  await loadData()
  
  console.log('[WordExamplesView] After loadData - allExamples:', allExamples.value.length, 'displayed:', displayedExamples.value.length, 'hasMore:', hasMore.value)
  
  // Setup intersection observer after initial load
  if (hasMore.value) {
    await setupIntersectionObserver()
  } else {
    console.log('[WordExamplesView] No more items to load, skipping observer setup')
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
.word-examples-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 2rem;
  font-size: 0.95rem;
  flex-wrap: wrap;
}

.breadcrumb a {
  color: #74c0fc;
  text-decoration: none;
  transition: color 0.3s;
}

.breadcrumb a:hover {
  color: #4dabf7;
}

.breadcrumb span {
  color: #adb5bd;
}

.view-header {
  margin-bottom: 2rem;
  padding: 2rem;
  background: linear-gradient(135deg, rgba(22, 33, 62, 0.8) 0%, rgba(15, 52, 96, 0.6) 100%);
  border-radius: 12px;
  border: 1px solid rgba(116, 192, 252, 0.2);
}

.word-info {
  display: flex;
  align-items: baseline;
  gap: 1rem;
  margin-bottom: 1rem;
}

.word-info h1 {
  font-size: 2.5rem;
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.phonetic {
  color: #adb5bd;
  font-size: 1.2rem;
  font-style: italic;
}

.meaning {
  color: #e9ecef;
  font-size: 1.2rem;
  margin-bottom: 0.5rem;
}

.vietnamese {
  color: #adb5bd;
  font-size: 1rem;
}

.loading, .error, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.error {
  color: #ff6b6b;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  gap: 1rem;
  flex-wrap: wrap;
}

.section-header h2 {
  font-size: 1.8rem;
  color: #74c0fc;
  margin: 0;
}

.load-more-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 0.95rem;
  font-weight: 600;
  transition: all 0.3s;
  white-space: nowrap;
}

.load-more-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(116, 192, 252, 0.4);
}

.load-more-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.examples-grid {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.empty-state {
  color: #adb5bd;
}

.load-more-trigger {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  gap: 0.5rem;
  color: #74c0fc;
  font-size: 0.95rem;
}

.all-loaded {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  gap: 0.5rem;
  color: #51cf66;
  font-size: 0.95rem;
}
</style>
