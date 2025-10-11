import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { postsService } from '@/plugins/services/services'
import { usePostsStore } from '../../stores/posts/posts'

export function usePosts() {
  const postsStore = usePostsStore()
  const { execute, isLoading } = usePromiseWrapper({
    key: 'posts',
    promiseFn: async (_, signal?: AbortSignal) => {
      const posts = await postsService.getPosts(signal)
      postsStore.setPosts(posts)
      return posts
    },
    // successMessage: 'get posts success'
  })
  const { execute: createPost } = usePromiseWrapper({
    key: 'createPosts',
    promiseFn: async () => {
      await postsService.createPost()
    },
    // successMessage: 'get posts success'
  })
  execute()
  return {
    posts: computed(() => postsStore.posts),
    isLoading,
    execute,
    createPost
  }
}
