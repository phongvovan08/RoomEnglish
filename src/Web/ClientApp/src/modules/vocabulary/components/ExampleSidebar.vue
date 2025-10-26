<template>
  <div class="example-sidebar">
    <div class="sidebar-header">
      <h4>📚 Examples in Group {{ groupIndex + 1 }}</h4>
      <div class="progress-info">
        {{ completedCount }}/{{ examples.length }} completed
      </div>
    </div>
    
    <div class="example-list">
      <div 
        v-for="(example, index) in examples" 
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
            v-if="isCompleted(index)" 
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, nextTick } from 'vue'
import { Icon } from '@iconify/vue'
import type { VocabularyExample } from '../types/vocabulary.types'

interface Props {
  examples: VocabularyExample[]
  groupIndex: number
  currentIndex: number
  completedExamples: number[] // Array of global indices
  groupStartIndex: number // Starting index of the group in all examples
}

const props = defineProps<Props>()

defineEmits<{
  selectExample: [index: number]
}>()

const itemRefs = ref<(HTMLElement | null)[]>([])

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

// Watch for currentIndex changes and scroll to the current item
watch(() => props.currentIndex, async (newIndex) => {
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
.example-sidebar {
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

.example-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
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
.example-sidebar::-webkit-scrollbar {
  width: 6px;
}

.example-sidebar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.example-sidebar::-webkit-scrollbar-thumb {
  background: rgba(116, 192, 252, 0.5);
  border-radius: 3px;
}

.example-sidebar::-webkit-scrollbar-thumb:hover {
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
