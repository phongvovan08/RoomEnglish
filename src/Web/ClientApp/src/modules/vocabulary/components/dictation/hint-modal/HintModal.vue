<template>
  <div v-if="show" class="hint-modal" @click="$emit('close')">
    <div class="hint-content" @click.stop>
      <div class="hint-header">
        <h3>💡 Hint</h3>
        <button @click="$emit('close')" class="close-btn">
          <i class="mdi mdi-close"></i>
        </button>
      </div>
      <div class="hint-list">
        <div class="hint-item">
          <i class="mdi mdi-information"></i>
          <span>The sentence has {{ wordCount }} words</span>
        </div>
        <div class="hint-item" v-if="firstLetter">
          <i class="mdi mdi-format-letter-case"></i>
          <span>First word starts with "{{ firstLetter }}"</span>
        </div>
        <div class="hint-item">
          <i class="mdi mdi-volume-high"></i>
          <span>Listen carefully to pronunciation and punctuation</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  show: boolean
  sentence?: string
}

const props = defineProps<Props>()

defineEmits<{
  close: []
}>()

const wordCount = props.sentence?.split(' ').length || 0
const firstLetter = props.sentence?.split(' ')[0]?.charAt(0) || ''
</script>

<style scoped>
.hint-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.hint-content {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.95) 0%, rgba(22, 33, 62, 0.95) 100%);
  border: 1px solid rgba(255, 193, 7, 0.3);
  border-radius: 15px;
  padding: 2rem;
  max-width: 500px;
  width: 90%;
  backdrop-filter: blur(20px);
}

.hint-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.hint-header h3 {
  color: #ffc107;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: #b8b8b8;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.hint-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.hint-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 10px;
  color: white;
}

.hint-item i {
  color: #ffc107;
  font-size: 1.2rem;
}
</style>
