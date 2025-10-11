import type { LocalStorageRepository } from '@/infrastructure/repositories/storages/localStorageRepository'

export class LocalStorageService {
  constructor(private readonly repository: LocalStorageRepository) {}

  isJsonString(str: string) {
    return this.repository.isJsonString(str)
  }
  getItem<T = string>(key: string) {
    return this.repository.getItem<T>(key)
  }
  setItem<T>(key: string, data: T) {
    return this.repository.setItem<T>(key, data)
  }
  removeItem = (key: string) => {
    return this.repository.removeItem(key)
  }
}
