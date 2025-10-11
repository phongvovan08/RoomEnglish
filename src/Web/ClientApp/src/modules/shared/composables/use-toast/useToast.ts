import { useToast as usePrimeVueToast, type ToastMessageOptions } from 'primevue'

export function useToast() {
  const toast = usePrimeVueToast()
  const defaultToastOptions: ToastMessageOptions = {
    closable: false,
    life: 3000
  }
  function error(summary: string, options: ToastMessageOptions = {}) {
    toast.add({
      severity: 'error',
      summary,
      ...defaultToastOptions,
      ...options
    })
  }
  function warn(summary: string, options: ToastMessageOptions = {}): void {
    toast.add({ severity: 'warn', summary, ...defaultToastOptions, ...options })
  }
  function info(summary: string, options: ToastMessageOptions = {}): void {
    toast.add({ severity: 'info', summary, ...defaultToastOptions, ...options })
  }
  function success(summary: string, options: ToastMessageOptions = {}): void {
    toast.add({ severity: 'success', summary, ...defaultToastOptions, ...options })
  }
  return { error, warn, info, success }
}
