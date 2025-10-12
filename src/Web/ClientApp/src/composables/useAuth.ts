import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService, type LoginRequest, type RegisterRequest, type UserInfo } from '@/services/authService'
import { useNotifications } from './useNotifications'

const user = ref<UserInfo | null>(null)
const isLoading = ref(false)
const isInitialized = ref(false)

export function useAuth() {
  const router = useRouter()
  const { success, error: showError } = useNotifications()

  const isAuthenticated = computed(() => !!user.value && AuthService.isAuthenticated())

  const initAuth = async () => {
    if (isInitialized.value) return

    try {
      if (AuthService.isAuthenticated()) {
        const userInfo = await AuthService.getUserInfo()
        user.value = userInfo
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

      const authResponse = await AuthService.login(credentials)
      AuthService.saveTokens(authResponse)

      // Get user info
      const userInfo = await AuthService.getUserInfo()
      user.value = userInfo

      success('Login Successful! 🎮', `Welcome back, ${userInfo.email}!`)
      
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
      
      success('Registration Successful! 🎉', 'Please check your email to confirm your account.')
      
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
      
      await AuthService.logout()
      user.value = null
      
      success('Logged Out Successfully! 👋', 'See you next time!')
      
      // Redirect to login page
      router.push('/auth/login')
      
    } catch (error) {
      console.error('Logout error:', error)
      // Even if logout fails, clear local state
      user.value = null
      AuthService.clearTokens()
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
    user: computed(() => user.value),
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