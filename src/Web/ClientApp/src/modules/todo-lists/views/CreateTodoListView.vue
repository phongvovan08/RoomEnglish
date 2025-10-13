<template>
  <div class="create-todo-list-container">
    <!-- Header Section -->
    <div class="header-section">
      <div class="flex items-center space-x-4 mb-8">
        <router-link
          :to="Routes.TodoLists.path"
          class="gaming-btn-secondary flex items-center p-2"
        >
          <Icon icon="mdi:arrow-left" class="w-5 h-5" />
        </router-link>
        <div>
          <h1 class="text-4xl font-bold text-white gaming-glow">{{ $t('todoLists.create.title') }}</h1>
          <p class="text-gray-400 mt-2">Create a new gaming-style todo list</p>
        </div>
      </div>
    </div>
    
    <!-- Gaming Form Card -->
    <div class="gaming-card">
      <form @submit.prevent="handleCreateTodoList" class="space-y-8">
        <!-- Title Field -->
        <div class="form-group">
          <label for="title" class="gaming-label">
            Title
          </label>
          <div class="input-wrapper">
            <input
              id="title"
              v-model="form.title"
              type="text"
              required
              class="gaming-input"
              placeholder="Enter todo list title"
            />
            <div class="input-glow"></div>
          </div>
        </div>

        <!-- Color Field -->
        <div class="form-group">
          <label for="colour" class="gaming-label">
            Color
          </label>
          <div class="select-wrapper">
            <select
              id="colour"
              v-model="form.colour"
              class="gaming-select"
            >
              <option value="Blue">Blue</option>
              <option value="Green">Green</option>
              <option value="Red">Red</option>
              <option value="Yellow">Yellow</option>
              <option value="Purple">Purple</option>
            </select>
            <div class="select-glow"></div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="flex justify-end space-x-4 pt-6">
          <router-link
            :to="Routes.TodoLists.path"
            class="gaming-btn-secondary"
          >
            {{ $t('common.cancel') }}
          </router-link>
          <button
            type="submit"
            :disabled="loading"
            class="gaming-btn-primary"
          >
            <span v-if="loading" class="flex items-center">
              <Icon icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              {{ $t('common.creating') }}
            </span>
            <span v-else class="flex items-center">
              <Icon icon="mdi:plus" class="w-4 h-4 mr-2" />
              {{ $t('common.create') }}
            </span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { useTodoLists } from '@/composables/useTodoLists'

const router = useRouter()
const { createTodoList: createTodoListAPI, loading } = useTodoLists()

const form = ref({
  title: '',
  colour: 'Blue'
})

const handleCreateTodoList = async () => {
  const success = await createTodoListAPI({
    title: form.value.title,
    colour: form.value.colour
  })
  
  if (success) {
    router.push(Routes.TodoLists.path)
  }
}
</script>

<style scoped>
/* Gaming Container */
.create-todo-list-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 2rem;
}

/* Header Section */
.header-section {
  margin-bottom: 2rem;
}

.gaming-glow {
  text-shadow: 0 0 10px #e75e8d, 0 0 20px #e75e8d, 0 0 30px #e75e8d;
}

/* Gaming Card */
.gaming-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 
    0 8px 32px rgba(0, 0, 0, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
  position: relative;
  overflow: hidden;
  max-width: 600px;
  margin: 0 auto;
}

.gaming-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(231, 94, 141, 0.1), transparent);
  transition: left 0.5s;
}

.gaming-card:hover::before {
  left: 100%;
}

/* Form Groups */
.form-group {
  margin-bottom: 2rem;
}

.gaming-label {
  display: block;
  color: #e75e8d;
  font-weight: 600;
  font-size: 1rem;
  margin-bottom: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

/* Input Styles */
.input-wrapper, .select-wrapper {
  position: relative;
}

.gaming-input, .gaming-select {
  width: 100%;
  padding: 1rem 1.25rem;
  background: rgba(0, 0, 0, 0.3);
  border: 2px solid rgba(231, 94, 141, 0.3);
  border-radius: 12px;
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
}

.gaming-input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.gaming-input:focus, .gaming-select:focus {
  outline: none;
  border-color: #e75e8d;
  box-shadow: 0 0 20px rgba(231, 94, 141, 0.3);
  transform: translateY(-2px);
}

.gaming-select {
  cursor: pointer;
}

.gaming-select option {
  background: #1a1a2e;
  color: white;
}

/* Input Glow Effects */
.input-glow, .select-glow {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  border-radius: 12px;
  background: linear-gradient(45deg, #e75e8d, #764ba2);
  opacity: 0;
  z-index: -1;
  transition: opacity 0.3s ease;
  filter: blur(20px);
}

.gaming-input:focus + .input-glow,
.gaming-select:focus + .select-glow {
  opacity: 0.3;
}

/* Gaming Buttons */
.gaming-btn-primary {
  background: linear-gradient(135deg, #e75e8d 0%, #764ba2 100%);
  color: white;
  padding: 0.875rem 2rem;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(231, 94, 141, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 140px;
}

.gaming-btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.4);
}

.gaming-btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.gaming-btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  padding: 0.875rem 2rem;
  border: 2px solid rgba(231, 94, 141, 0.3);
  border-radius: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 120px;
  backdrop-filter: blur(10px);
}

.gaming-btn-secondary:hover {
  background: rgba(231, 94, 141, 0.1);
  border-color: #e75e8d;
  transform: translateY(-2px);
  text-decoration: none;
  color: white;
}

/* Animations */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* Error Message */
.error-message {
  color: #ff6b6b;
  font-size: 0.875rem;
  margin-top: 0.5rem;
}

.gaming-input.error, .gaming-select.error {
  border-color: #ff6b6b;
  box-shadow: 0 0 20px rgba(255, 107, 107, 0.3);
}

/* Responsive Design */
@media (max-width: 768px) {
  .create-todo-list-container {
    padding: 1rem;
  }
  
  .gaming-card {
    padding: 1.5rem;
  }
  
  .flex.justify-end {
    flex-direction: column;
    gap: 1rem;
  }
  
  .gaming-btn-primary,
  .gaming-btn-secondary {
    width: 100%;
  }
}

/* Loading Animation */
@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}
</style>