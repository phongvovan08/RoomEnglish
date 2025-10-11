import { PromiseStates, type QueryOptions } from '@/modules/shared/types'
import { usePromiseState } from '../use-promise-state'
import { useQueryStore } from '../../stores/query/query'
import { useToast } from '../use-toast/useToast'

export function usePromiseWrapper<TParams = void, TResult = any, TError = any>({
  key,
  promiseFn,
  cancellable = true,
  globalLoading = false,
  errorHandler,
  successMessage
}: QueryOptions<TParams, TResult, TError>) {
  const { isLoading, state, setState } = usePromiseState(key)
  const queryStore = useQueryStore()
  const toast = useToast()
  const errHandler =
    errorHandler ??
    ((error: any) => {
      const errorMessage = error.response?.data?.message ?? error.message
      console.error(errorMessage)
      toast.error(errorMessage)
    })

  async function execute(params: TParams) {
    try {
      if (cancellable) {
        const abortController = queryStore.abortControllersMap.get(key)
        if (abortController?.signal) {
          abortController.abort()
        }
        queryStore.abortControllersMap.set(key, new AbortController())
      }
      setState(PromiseStates.pending)
      queryStore.setGlobalLoadingIfActivated(true, globalLoading)
      const res = await promiseFn(params, queryStore.abortControllersMap?.get(key)?.signal)
      queryStore.abortControllersMap.delete(key)

      setState(PromiseStates.success)
      if (successMessage) {
        toast.success(successMessage)
      }
      queryStore.setGlobalLoadingIfActivated(false, globalLoading)
      return res as TResult
    } catch (err: any) {
      setState(PromiseStates.error)
      queryStore.setGlobalLoadingIfActivated(false, globalLoading)
      const errorCode = err.code
      if (cancellable && errorCode === 'ERR_CANCELED') {
        return
      }
      queryStore.abortControllersMap.delete(key)
      errHandler(err)
    }
  }

  return {
    isLoading,
    state,
    execute
  }
}
