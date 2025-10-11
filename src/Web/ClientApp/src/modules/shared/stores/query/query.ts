import type { PromiseStates } from '../../types'

export const useQueryStore = defineStore('query', () => {
  const cachedDataMap = reactive(
    new Map<
      string,
      {
        data: unknown
        at: Date
      }
    >()
  )
  const statesMap = reactive(new Map<string, PromiseStates>())
  const abortControllersMap = reactive(new Map<string, AbortController>())
  const isLoading = ref(false)
  function setGlobalLoadingIfActivated(value: boolean, globalLoading: boolean) {
    if (globalLoading) {
      isLoading.value = value
    }
  }
  function setLoading(value: boolean) {
    isLoading.value = value
  }
  return {
    cachedDataMap,
    statesMap,
    abortControllersMap,
    isLoading,
    setLoading,
    setGlobalLoadingIfActivated
  }
})
