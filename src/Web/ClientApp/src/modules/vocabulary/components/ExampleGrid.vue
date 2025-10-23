<template>
  <div class="example-grid-container">
    <div class="grid-header">
      <h3>📝 Choose an Example to Practice</h3>
      <p class="word-info">Word: <strong>{{ word.word }}</strong></p>
    </div>

    <div class="examples-grid">
      <div 
        v-for="(example, index) in examples" 
        :key="index"
        class="example-card"
        @click="$emit('select', index)"
      >
        <div class="example-number">{{ index + 1 }}</div>
        <div class="example-text">{{ example.sentence }}</div>
        <div class="example-footer">
          <span class="click-hint">Click to practice</span>
          <Icon icon="mdi:arrow-right-circle" class="arrow-icon" />
        </div>
      </div>
    </div>

    <div class="grid-actions">
      <button @click="$emit('back')" class="back-button">
        <Icon icon="mdi:arrow-left" />
        Back to Word
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'
import type { VocabularyWord, VocabularyExample } from '../types/vocabulary.types'

interface Props {
  word: VocabularyWord
  examples: VocabularyExample[]
}

defineProps<Props>()

defineEmits<{
  select: [index: number]
  back: []
}>()
</script>

<style scoped>
.example-grid-container {
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  padding: 2rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.grid-header {
  text-align: center;
  margin-bottom: 2rem;
}

.grid-header h3 {
  color: white;
  font-size: 1.8rem;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.word-info {
  color: #b8b8b8;
  font-size: 1rem;
}

.word-info strong {
  color: #74c0fc;
  font-size: 1.2rem;
}

.examples-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.example-card {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.example-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(231, 94, 141, 0.1), rgba(116, 192, 252, 0.1));
  opacity: 0;
  transition: opacity 0.3s ease;
}

.example-card:hover {
  border-color: rgba(231, 94, 141, 0.5);
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(231, 94, 141, 0.3);
}

.example-card:hover::before {
  opacity: 1;
}

.example-number {
  position: relative;
  z-index: 1;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  width: 35px;
  height: 35px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.example-text {
  position: relative;
  z-index: 1;
  color: white;
  font-size: 1rem;
  line-height: 1.6;
  margin-bottom: 1rem;
  min-height: 60px;
}

.example-footer {
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.click-hint {
  color: #74c0fc;
  font-size: 0.85rem;
  font-weight: 500;
}

.arrow-icon {
  color: #e75e8d;
  font-size: 1.5rem;
  transition: transform 0.3s ease;
}

.example-card:hover .arrow-icon {
  transform: translateX(5px);
}

.grid-actions {
  text-align: center;
}

.back-button {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.back-button:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

@media (max-width: 768px) {
  .examples-grid {
    grid-template-columns: 1fr;
  }
  
  .example-grid-container {
    padding: 1rem;
  }
  
  .grid-header h3 {
    font-size: 1.4rem;
  }
}
</style>
