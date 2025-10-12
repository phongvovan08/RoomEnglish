<template>
  <div class="space-y-6">
    <div class="flex items-center space-x-4">
      <router-link
        :to="Routes.TodoLists.path"
        class="text-blue-600 hover:text-blue-800"
      >
        <Icon icon="mdi:arrow-left" class="w-5 h-5" />
      </router-link>
      <h1 class="text-3xl font-bold text-gray-900">{{ $t('todoLists.create.title') }}</h1>
    </div>
    
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <form @submit.prevent="handleCreateTodoList" class="space-y-6">
        <div>
          <label for="title" class="block text-sm font-medium text-gray-700 mb-2">
            Title
          </label>
          <input
            id="title"
            v-model="form.title"
            type="text"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="Enter todo list title"
          />
        </div>
        
        <div>
          <label for="colour" class="block text-sm font-medium text-gray-700 mb-2">
            Color
          </label>
          <select
            id="colour"
            v-model="form.colour"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="Blue">Blue</option>
            <option value="Green">Green</option>
            <option value="Red">Red</option>
            <option value="Yellow">Yellow</option>
            <option value="Purple">Purple</option>
          </select>
        </div>
        
        <div class="flex justify-end space-x-4">
          <router-link
            :to="Routes.TodoLists.path"
            class="px-4 py-2 text-gray-700 bg-gray-200 rounded-lg hover:bg-gray-300 transition-colors duration-200"
          >
            {{ $t('common.cancel') }}
          </router-link>
          <button
            type="submit"
            :disabled="loading"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors duration-200"
          >
            {{ loading ? $t('common.creating') : $t('common.create') }}
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