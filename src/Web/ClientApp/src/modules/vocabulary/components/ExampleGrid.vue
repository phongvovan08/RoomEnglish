<template>
  <div class="example-grid-container">
    <div class="grid-header">
      <h3>📝 Choose a Group to Practice</h3>
      <p class="word-info">Word: <strong>{{ word.word }}</strong></p>
      <p class="total-info">{{ examples.length }} examples in {{ groupedExamples.length }} groups</p>
    </div>

    <div class="examples-grid">
      <div 
        v-for="(group, groupIndex) in groupedExamples" 
        :key="groupIndex"
        class="example-group-card"
        @click="selectGroup(groupIndex)"
      >
        <div class="group-header">
          <div class="group-number">{{ groupIndex + 1 }}</div>
          <div class="group-title">
            Group {{ groupIndex + 1 }}
          </div>
        </div>
        
        <div class="group-range">
          Examples {{ group.startIndex + 1 }} - {{ group.endIndex + 1 }}
        </div>
        
        <div class="group-count">
          {{ group.examples.length }} sentences
        </div>

        <div class="progress-section">
          <div class="progress-bar">
            <div 
              class="progress-fill" 
              :style="{ width: `${group.completionPercentage}%` }"
            ></div>
          </div>
          <div class="progress-text">{{ Math.round(group.completionPercentage) }}% completed</div>
        </div>

        <div class="group-footer">
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
import { computed } from 'vue'
import { Icon } from '@iconify/vue'
import type { VocabularyWord, VocabularyExample } from '../types/vocabulary.types'

interface Props {
  word: VocabularyWord
  examples: VocabularyExample[]
  completedExamples?: number[] // Array of completed example indices
}

const props = withDefaults(defineProps<Props>(), {
  completedExamples: () => []
})

const emit = defineEmits<{
  selectGroup: [groupIndex: number]
  back: []
}>()

interface ExampleGroup {
  examples: VocabularyExample[]
  startIndex: number
  endIndex: number
  completionPercentage: number
}

const groupedExamples = computed<ExampleGroup[]>(() => {
  const groups: ExampleGroup[] = []
  const groupSize = 10
  
  for (let i = 0; i < props.examples.length; i += groupSize) {
    const groupExamples = props.examples.slice(i, i + groupSize)
    const startIndex = i
    const endIndex = Math.min(i + groupSize - 1, props.examples.length - 1)
    
    // Calculate completion percentage for this group
    const completedCount = groupExamples.filter((_, index) => 
      props.completedExamples.includes(startIndex + index)
    ).length
    const completionPercentage = (completedCount / groupExamples.length) * 100
    
    groups.push({
      examples: groupExamples,
      startIndex,
      endIndex,
      completionPercentage
    })
  }
  
  return groups
})

const selectGroup = (groupIndex: number) => {
  emit('selectGroup', groupIndex)
}
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

.total-info {
  color: #b8b8b8;
  font-size: 0.9rem;
  margin-top: 0.5rem;
}

.examples-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.example-group-card {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.example-group-card::before {
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

.example-group-card:hover {
  border-color: rgba(231, 94, 141, 0.5);
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(231, 94, 141, 0.3);
}

.example-group-card:hover::before {
  opacity: 1;
}

.group-header {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}

.group-number {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  width: 45px;
  height: 45px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 1.2rem;
  flex-shrink: 0;
}

.group-title {
  color: white;
  font-size: 1.2rem;
  font-weight: 600;
}

.group-range {
  position: relative;
  z-index: 1;
  color: #74c0fc;
  font-size: 0.95rem;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.group-count {
  position: relative;
  z-index: 1;
  color: #b8b8b8;
  font-size: 0.85rem;
  margin-bottom: 1rem;
}

.progress-section {
  position: relative;
  z-index: 1;
  margin-bottom: 1rem;
}

.progress-bar {
  height: 8px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #e75e8d, #74c0fc);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.progress-text {
  color: #74c0fc;
  font-size: 0.85rem;
  font-weight: 500;
}

.group-footer {
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

.example-group-card:hover .arrow-icon {
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
