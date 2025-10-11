export class LocalStorageRepository {
  get hasWindow() {
    if (typeof window !== 'undefined') {
      return true
    }
    return false
  }
  isJsonString(str: string) {
    try {
      JSON.parse(str)
      //eslint-disable-next-line
    } catch (_) {
      return false
    }
    return true
  }
  getItem<T = string>(key: string) {
    if (this.hasWindow) {
      const raw = window.localStorage.getItem(key)
      if (raw) {
        if (this.isJsonString(raw)) {
          return JSON.parse(raw) as T
        }
        return raw as T
      }
      return undefined
    }
    return undefined
  }
  setItem<T>(key: string, data: T) {
    if (this.hasWindow) {
      window.localStorage.setItem(key, typeof data === 'string' ? data : JSON.stringify(data))
    }
  }
  removeItem = (key: string) => {
    if (this.hasWindow) {
      window.localStorage.removeItem(key)
    }
  }
}
