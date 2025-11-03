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
      <h2>Practice Examples</h2>
      <div class="examples-grid">
        <DictationCard
          v-for="(example, index) in examples"
          :key="example.id"
          :example="example"
          :index="index"
          @next="handleNext(index)"
        />
      </div>
    </div>

    <div v-if="!loading && examples.length === 0" class="empty-state">
      <Icon icon="mdi:text-box-outline" class="w-16 h-16 opacity-50" />
      <p>No examples found for this word</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
const examples = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

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
      examples.value = word.value.examples
    }
    
    console.log('[WordExamplesView] Examples loaded:', examples.value.length, 'examples')
  } catch (err: any) {
    console.error('Failed to load word examples:', err)
    error.value = err.message || 'Failed to load examples'
  } finally {
    loading.value = false
  }
}

const handleNext = (currentIndex: number) => {
  console.log('Next example after:', currentIndex)
  // Could add logic to auto-scroll to next card
}

onMounted(() => {
  loadData()
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

.examples-section h2 {
  font-size: 1.8rem;
  color: #74c0fc;
  margin-bottom: 1.5rem;
}

.examples-grid {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.empty-state {
  color: #adb5bd;
}
</style>
