<script lang="ts" setup>
import { usePosts } from '../composables/use-posts/usePosts'
import Post from '../components/post/Post.vue'
import Button from '@/modules/shared/components/button'

const { posts, execute, isLoading, createPost } = usePosts()
</script>

<template>
  <Button @click="() => {
    execute()
  }" class="mb-4" >Refresh posts</Button>
   <Button @click="() => {
    createPost()
  }" class="mb-4" >Create posts</Button>

  <div v-if="isLoading">Loading...</div>
  <div v-else class="grid grid-cols-3 gap-4 h-[calc(100vh-200px)] overflow-auto w-full">
    <Post v-for="post in posts" :key="post.id" :post> </Post>
  </div>
</template>
