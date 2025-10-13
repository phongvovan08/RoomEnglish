import type { App } from 'vue'
import { AuthService } from '@/services/authService'

export default {
  install(app: App) {
    // Initialize authentication when the app starts
    // Note: useAuth should only be called within component setup functions
    
    // Initialize auth state using service directly
    AuthService.isAuthenticated() // This will validate tokens
  }
}