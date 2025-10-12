import type { App } from 'vue'
import { useAuth } from '@/composables/useAuth'

export default {
  install(app: App) {
    // Initialize authentication when the app starts
    app.config.globalProperties.$auth = useAuth()
    
    // Initialize auth state
    const { initAuth } = useAuth()
    initAuth().catch(error => {
      console.warn('Failed to initialize auth on app start:', error)
    })
  }
}