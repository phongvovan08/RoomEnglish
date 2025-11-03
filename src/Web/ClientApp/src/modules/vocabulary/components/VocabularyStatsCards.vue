<template>
  <div class="statistics-grid">
    <div class="stat-card">
      <div class="stat-icon">
        <i class="mdi mdi-book"></i>
      </div>
      <div class="stat-content">
        <h3>{{ stats.totalWords }}</h3>
        <p>Total Words</p>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">
        <i class="mdi mdi-format-quote-close"></i>
      </div>
      <div class="stat-content">
        <h3>{{ stats.totalExamples }}</h3>
        <p>Total Examples</p>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">
        <i class="mdi mdi-tag"></i>
      </div>
      <div class="stat-content">
        <h3>{{ stats.totalCategories }}</h3>
        <p>Categories</p>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">
        <i class="mdi mdi-clock"></i>
      </div>
      <div class="stat-content">
        <h3>{{ stats.lastUpdated }}</h3>
        <p>Last Updated</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { VocabularyStats } from '../composables/use-vocabulary-management/useVocabularyManagement'

defineProps<{
  stats: VocabularyStats
}>()
</script>

<style scoped>
.statistics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1) 0%, rgba(255, 255, 255, 0.05) 100%);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: transform 0.3s, box-shadow 0.3s;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  font-size: 2rem;
  color: white;
}

.stat-content h3 {
  font-size: 2rem;
  font-weight: bold;
  margin-bottom: 0.25rem;
  color: #e9ecef;
}

.stat-content p {
  color: #adb5bd;
  font-size: 0.9rem;
}
</style>
