import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthService } from '@/services/authService'

interface User {
  id: string
  email: string
  firstName?: string
  lastName?: string
  displayName?: string
  roles: string[]
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const isLoading = ref(false)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userDisplayName = computed(() => user.value?.displayName || user.value?.email || 'User')
  const hasRole = (role: string) => user.value?.roles?.includes(role) ?? false

  // Actions
  const setUser = (userData: User) => {
    user.value = userData
  }

  const setToken = (tokenValue: string) => {
    token.value = tokenValue
  }

  const clearAuth = () => {
    user.value = null
    token.value = null
    AuthService.clearTokens()
  }

  const initializeAuth = () => {
    const storedToken = AuthService.getToken()
    if (storedToken && !AuthService.isTokenExpired()) {
      token.value = storedToken
      // Load user data
      loadUserProfile()
    } else {
      // Clear expired tokens
      AuthService.clearTokens()
      clearAuth()
    }
  }

  const loadUserProfile = async () => {
    if (!token.value) return

    try {
      isLoading.value = true
      const response = await fetch('/api/users/me', {
        headers: {
          'Authorization': `Bearer ${token.value}`,
          'Content-Type': 'application/json'
        }
      })

      if (response.ok) {
        const userData = await response.json()
        setUser(userData)
      } else if (response.status === 401) {
        // Token is invalid, clear auth
        clearAuth()
      }
    } catch (error) {
      console.error('Failed to load user profile:', error)
    } finally {
      isLoading.value = false
    }
  }

  const login = async (email: string, password: string) => {
    try {
      isLoading.value = true
      const response = await AuthService.login({ email, password })
      
      // Save tokens using AuthService
      AuthService.saveTokens(response)
      token.value = response.accessToken
      
      // Load user profile after successful login
      await loadUserProfile()
      
      return { success: true }
    } catch (error: any) {
      console.error('Login error:', error)
      return { success: false, error: error.message || 'Login failed' }
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    try {
      await AuthService.logout()
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      clearAuth()
    }
  }

  const register = async (registerData: {
    email: string
    password: string
    firstName?: string
    lastName?: string
  }) => {
    try {
      isLoading.value = true
      await AuthService.register({
        email: registerData.email,
        password: registerData.password
      })
      
      // After successful registration, login the user
      return await login(registerData.email, registerData.password)
    } catch (error: any) {
      console.error('Registration error:', error)
      return { success: false, error: error.message || 'Registration failed' }
    } finally {
      isLoading.value = false
    }
  }

  return {
    // State
    user,
    token,
    isLoading,
    
    // Getters
    isAuthenticated,
    userDisplayName,
    hasRole,
    
    // Actions
    setUser,
    setToken,
    clearAuth,
    initializeAuth,
    loadUserProfile,
    login,
    logout,
    register
  }
})