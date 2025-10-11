import type { Post } from '@/domain/entities/posts'

export const usePostsStore = defineStore('posts', () => {
  const posts = ref<Post[]>([])
  function setPosts(items: Post[]) {
    posts.value = [...items]
  }
  return {
    posts,
    setPosts
  }
})
