import { RepositoryFactory } from '@/infrastructure/repositories/repositoryFactory'
import { PostsRepository } from '@/infrastructure/repositories/posts/postsRepository'
import { PostsService } from '@/domain/services/posts/postsService'
import { MsalService } from '@/domain/services/msal/msalService'
import { ERepositories } from '@/infrastructure/constants'
import type { MsalRepository } from '@/infrastructure/repositories/msal/msalRepository'
import { LocalStorageService } from '@/domain/services/storages/localStorageService'
import type { LocalStorageRepository } from '@/infrastructure/repositories/storages/localStorageRepository'

const msalRepository = RepositoryFactory.getRepository(ERepositories.Msal) as MsalRepository
const postsRepository = RepositoryFactory.getRepository(ERepositories.Posts) as PostsRepository
const localStorageRepository = RepositoryFactory.getRepository(ERepositories.LocalStorage) as LocalStorageRepository
const postsService = new PostsService(postsRepository)
const msalService = new MsalService(msalRepository)
const localStorageService = new LocalStorageService(localStorageRepository)

export { postsService, msalService, localStorageService }
