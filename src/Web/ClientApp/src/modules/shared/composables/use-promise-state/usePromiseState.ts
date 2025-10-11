import { PromiseStates } from '@/modules/shared/types'
import { useQueryStore } from '../../stores/query/query'


export function usePromiseState(key: string) {
  const store = useQueryStore()
  function setState(state: PromiseStates) {
    store.statesMap.set(key, state)
  }
  function matches(state: PromiseStates) {
    return store.statesMap.get(key) === state
  }
  const state = computed(() => store.statesMap.get(key) ?? PromiseStates.idle)
  const isLoading = computed(() => matches(PromiseStates.pending))

  return {
    isLoading,
    state,
    setState,
    matches,
  }
}