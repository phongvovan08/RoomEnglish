<script lang="ts" setup>
import type { Post } from "@/domain/entities/posts";

defineProps<{ post: Post }>();


</script>

<template>
  <div
    class="space-y-4 border border-solid border-gray-200 shadow-ld rounded-md p-4"
  >
    <div class="text-xl font-bold">{{ post.title }}</div>
    <div>{{ post.body }}</div>
  </div>
</template>
