import { type QueryOptions } from '@/modules/shared/types'
import { useQueryStore } from '../../stores/query/query'
import { usePromiseWrapper } from '../use-promise-wrapper'

export function useQuery<TParams = void, TResult = any, TError = any>({
  key,
  promiseFn,
  cancellable = true,
  secondsToLive, // 0 or undefined the cache will be live forever until you refresh it
  errorHandler
}: QueryOptions<TParams, TResult, TError> & { secondsToLive?: number }) {
  const queryStore = useQueryStore()
  const data = computed<TResult | undefined>(() => queryStore.cachedDataMap.get(key)?.data as TResult)
  const { execute, isLoading, state } = usePromiseWrapper({ key, promiseFn, errorHandler, cancellable })
  async function query(params: TParams, forceUpdate = false) {
    const cachedData = queryStore.cachedDataMap.get(key)
    if (cachedData && !forceUpdate) {
      const isCacheLive = secondsToLive ? new Date().getTime() - cachedData.at.getTime() <= secondsToLive * 1000 : true
      if (isCacheLive) {
        return Promise.resolve(queryStore.cachedDataMap.get(key)?.data as TResult)
      }
    }
    const result = await execute(params)
    queryStore.cachedDataMap.set(key, {
      data: result,
      at: new Date()
    })
    return result
  }

  async function refresh(params: TParams) {
    return query(params, true)
  }
  function setCachedData(value: TResult | undefined) {
    queryStore.cachedDataMap.set(key, { data: value, at: new Date() })
  }
  return {
    isLoading,
    data,
    state,
    execute: query,
    refresh,
    setCachedData
  }
}
