export enum PromiseStates {
  idle,
  pending,
  success,
  error,
}
export interface DefaultError {
  code: string
  message: string
}
export type Optional<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>
export type HTMLElementEvent<T extends HTMLElement> = Event & {
  target: T
}
export type SelectableItem<T = Record<string, any>> = {
  label: string
  value: string
} & T

export interface QueryOptions<TParams, TResult, TError> {
  promiseFn: (params: TParams, abortSignal?: AbortSignal) => Promise<TResult>
  key: string
  cancellable?: boolean
  errorHandler?: (error: TError) => void
  successMessage?: string
  globalLoading?: boolean
}