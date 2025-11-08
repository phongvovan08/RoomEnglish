import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService, type LoginRequest, type RegisterRequest, type UserInfo } from '@/services/authService'
import { useNotifications } from '@/utils/notifications'
import { useAuthStore } from '@/stores/auth'

const user = ref<UserInfo | null>(null)
const isLoading = ref(false)
const isInitialized = ref(false)

export function useAuth() {
  const router = useRouter()
  const { showSuccess, showError } = useNotifications()
  const authStore = useAuthStore()

  const isAuthenticated = computed(() => authStore.isAuthenticated)

  const initAuth = async () => {
    if (isInitialized.value) return

    try {
      if (AuthService.isAuthenticated()) {
        const userInfo = await AuthService.getUserInfo()
        user.value = userInfo
        
        // Sync with authStore
        const token = localStorage.getItem('access_token')
        if (token) {
          authStore.setToken(token)
          await authStore.loadUserProfile()
        }
      }
    } catch (error) {
      console.warn('Failed to initialize auth:', error)
      AuthService.clearTokens()
    } finally {
      isInitialized.value = true
    }
  }

  const login = async (credentials: LoginRequest) => {
    try {
      isLoading.value = true

      console.log('🔐 Logging in with useAuth...')
      const authResponse = await AuthService.login(credentials)
      AuthService.saveTokens(authResponse)

      // Get user info
      const userInfo = await AuthService.getUserInfo()
      user.value = userInfo
      
      // Sync with authStore
      const authStore = useAuthStore()
      authStore.setToken(authResponse.accessToken)
      await authStore.loadUserProfile()
      console.log('✅ Auth synced to authStore, user:', authStore.user)

      showSuccess('Login Successful! 🎮', `Welcome back, ${userInfo.email}!`)
      
      // Redirect to dashboard or intended route
      const redirectTo = router.currentRoute.value.query.redirect as string
      router.push(redirectTo || '/dashboard')

      return authResponse
    } catch (error) {
      console.error('Login error:', error)
      showError('Login Failed ❌', error instanceof Error ? error.message : 'Invalid credentials')
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const register = async (userData: RegisterRequest) => {
    try {
      isLoading.value = true

      await AuthService.register(userData)
      
  showSuccess('Registration Successful! 🎉', 'Please check your email to confirm your account.')
      
      // Redirect to login page
      router.push('/auth/login')
      
    } catch (error) {
      console.error('Registration error:', error)
      showError('Registration Failed ❌', error instanceof Error ? error.message : 'Registration failed')
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    try {
      isLoading.value = true
      
      console.log('🔓 Logging out...')
      await AuthService.logout()
      user.value = null
      
      // Also clear authStore
      const authStore = useAuthStore()
      authStore.clearAuth()
      console.log('✅ Auth cleared from both useAuth and authStore')
      
      showSuccess('Logged Out Successfully! 👋', 'See you next time!')
      
      // Redirect to login page
      router.push('/auth/login')
      
    } catch (error) {
      console.error('Logout error:', error)
      // Even if logout fails, clear local state
      user.value = null
      AuthService.clearTokens()
      
      // Clear authStore
      const authStore = useAuthStore()
      authStore.clearAuth()
      
      router.push('/auth/login')
    } finally {
      isLoading.value = false
    }
  }

  const refreshToken = async () => {
    try {
      const authResponse = await AuthService.refreshToken()
      AuthService.saveTokens(authResponse)
      return true
    } catch (error) {
      console.error('Token refresh failed:', error)
      await logout()
      return false
    }
  }

  const checkAuthStatus = async () => {
    if (!AuthService.isAuthenticated()) {
      user.value = null
      return false
    }

    if (AuthService.isTokenExpired()) {
      return await refreshToken()
    }

    return true
  }

  return {
    // State
    user: computed(() => authStore.user),
    isAuthenticated,
    isLoading: computed(() => isLoading.value),
    isInitialized: computed(() => isInitialized.value),

    // Actions
    initAuth,
    login,
    register,
    logout,
    refreshToken,
    checkAuthStatus
  }
}