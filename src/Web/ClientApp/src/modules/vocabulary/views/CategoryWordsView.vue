<template>
  <div class="category-words-view">
    <div class="breadcrumb">
      <router-link :to="{ name: 'VocabularyLearningCategories' }">
        <Icon icon="mdi:arrow-left" class="w-5 h-5" />
        Back to Categories
      </router-link>
      <span v-if="category"> / {{ category.name }}</span>
    </div>

    <div class="view-header">
      <h1>{{ category?.name || 'Loading...' }}</h1>
      <p v-if="category">{{ category.description }}</p>
    </div>

    <div v-if="loading" class="loading">
      <Icon icon="mdi:loading" class="animate-spin w-8 h-8" />
      Loading words...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <div v-else class="words-grid">
      <div 
        v-for="word in words" 
        :key="word.id"
        class="word-card"
        @click="goToWordExamples(word.id)"
      >
        <div class="word-header">
          <h3>{{ word.word }}</h3>
          <span class="phonetic" v-if="word.phonetic">
            {{ word.phonetic }}
          </span>
        </div>
        
        <div class="word-body">
          <p class="meaning">{{ word.meaning }}</p>
          <p class="vietnamese" v-if="word.vietnameseMeaning">
            {{ word.vietnameseMeaning }}
          </p>
        </div>

        <div class="word-footer">
          <span class="examples-count">
            <Icon icon="mdi:format-list-bulleted" />
            {{ word.exampleCount || 0 }} examples
          </span>
          <Icon icon="mdi:arrow-right" class="arrow-icon" />
        </div>
      </div>
    </div>

    <div v-if="!loading && words.length === 0" class="empty-state">
      <Icon icon="mdi:book-open-blank-variant" class="w-16 h-16 opacity-50" />
      <p>No words found in this category</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import { useVocabularyCategories } from '../composables/use-vocabulary-categories'
import { useVocabularyWords } from '../composables/use-vocabulary-words'

const route = useRoute()
const router = useRouter()

const { loadCategories, categories } = useVocabularyCategories()
const { loadWords, words: wordsData } = useVocabularyWords()

const categoryId = ref<number>(Number(route.query.categoryId))
const category = ref<any>(null)
const words = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const loadCategoryAndWords = async () => {
  loading.value = true
  error.value = null
  
  try {
    console.log('[CategoryWordsView] Loading category:', categoryId.value)
    
    // Load category details
    await loadCategories({})
    category.value = categories.value.find((c: any) => c.id === categoryId.value) || null
    console.log('[CategoryWordsView] Category loaded:', category.value)
    
    // Load words
    console.log('[CategoryWordsView] Loading words for category:', categoryId.value)
    await loadWords({
      categoryId: categoryId.value,
      pageNumber: 1,
      pageSize: 100
    })
    
    console.log('[CategoryWordsView] Words loaded:', wordsData.value.length, 'words')
    words.value = [...wordsData.value]
  } catch (err: any) {
    console.error('Failed to load category words:', err)
    error.value = err.message || 'Failed to load words'
  } finally {
    loading.value = false
  }
}

const goToWordExamples = (wordId: number) => {
  router.push({
    name: 'VocabularyLearningExamples',
    query: {
      wordId: wordId
    }
  })
}

onMounted(() => {
  loadCategoryAndWords()
})
</script>

<style scoped>
.category-words-view {
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
}

.breadcrumb a {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #74c0fc;
  text-decoration: none;
  transition: color 0.3s;
}

.breadcrumb a:hover {
  color: #4dabf7;
}

.view-header {
  margin-bottom: 2rem;
}

.view-header h1 {
  font-size: 2rem;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.view-header p {
  color: #adb5bd;
  font-size: 1.1rem;
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

.words-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.word-card {
  background: linear-gradient(135deg, rgba(22, 33, 62, 0.8) 0%, rgba(15, 52, 96, 0.6) 100%);
  border-radius: 12px;
  padding: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid rgba(116, 192, 252, 0.2);
  position: relative;
  overflow: hidden;
}

.word-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.1) 0%, transparent 100%);
  opacity: 0;
  transition: opacity 0.3s;
}

.word-card:hover {
  transform: translateY(-4px);
  border-color: rgba(116, 192, 252, 0.5);
  box-shadow: 0 8px 24px rgba(116, 192, 252, 0.2);
}

.word-card:hover::before {
  opacity: 1;
}

.word-header {
  margin-bottom: 1rem;
}

.word-header h3 {
  font-size: 1.5rem;
  color: #74c0fc;
  margin-bottom: 0.25rem;
}

.phonetic {
  color: #adb5bd;
  font-size: 0.9rem;
  font-style: italic;
}

.word-body {
  margin-bottom: 1rem;
}

.meaning {
  color: #e9ecef;
  margin-bottom: 0.5rem;
  font-size: 1rem;
}

.vietnamese {
  color: #adb5bd;
  font-size: 0.9rem;
}

.word-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid rgba(116, 192, 252, 0.2);
}

.examples-count {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #74c0fc;
  font-size: 0.9rem;
}

.arrow-icon {
  color: #74c0fc;
  transition: transform 0.3s;
}

.word-card:hover .arrow-icon {
  transform: translateX(4px);
}

.empty-state {
  color: #adb5bd;
}
</style>
