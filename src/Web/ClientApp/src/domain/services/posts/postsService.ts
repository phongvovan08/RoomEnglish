import type { Post } from '@/domain/entities/posts'
import type { PostsRepository } from '@/infrastructure/repositories/posts/postsRepository'

export class PostsService {
  constructor(private readonly repository: PostsRepository) {}

  getPosts(signal?: AbortSignal): Promise<Post[]> {
    return this.repository.get(undefined, undefined, {
      signal
    })
  }
  createPost() {
    return this.repository.create({
      title: 'Newly created post title' + Math.random().toString(),
      body: 'Newly created post body' + Math.random().toString(),
      userId: 1,
    })
  }
}
