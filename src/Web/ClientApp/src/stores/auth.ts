import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthService } from '@/services/authService'
import { API_CONFIG } from '@/config/api.config'

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
  
  // Methods
  const hasRole = (role: string) => {
    console.log('🔍 Checking role:', role, 'User roles:', user.value?.roles)
    return user.value?.roles?.includes(role) ?? false
  }

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
    console.log('🔐 Initializing auth...')
    console.log('Stored token:', storedToken ? `${storedToken.substring(0, 20)}...` : 'null')
    console.log('Token expired:', AuthService.isTokenExpired())
    
    if (storedToken && !AuthService.isTokenExpired()) {
      console.log('✅ Valid token found, loading user profile...')
      token.value = storedToken
      // Load user data
      loadUserProfile()
    } else {
      console.warn('⚠️ No valid token found, user needs to login')
      // Clear expired tokens
      AuthService.clearTokens()
      clearAuth()
    }
  }

  const loadUserProfile = async () => {
    if (!token.value) {
      console.log('⚠️ No token available for loadUserProfile')
      return
    }

    try {
      isLoading.value = true
      const apiUrl = API_CONFIG.baseURL ? `${API_CONFIG.baseURL}/api/users/me` : '/api/users/me'
      console.log('📡 Loading user profile from', apiUrl)
      
      const response = await fetch(apiUrl, {
        headers: {
          'Authorization': `Bearer ${token.value}`,
          'Content-Type': 'application/json'
        }
      })

      if (response.ok) {
        const userData = await response.json()
        console.log('✅ User profile loaded:', userData)
        console.log('User roles:', userData.roles)
        setUser(userData)
      } else if (response.status === 401) {
        console.warn('❌ Token is invalid (401), clearing auth')
        // Token is invalid, clear auth
        clearAuth()
      } else {
        console.error('❌ Failed to load user profile:', response.status)
      }
    } catch (error) {
      console.error('❌ Failed to load user profile:', error)
    } finally {
      isLoading.value = false
    }
  }

  const login = async (email: string, password: string) => {
    try {
      isLoading.value = true
      console.log('🔐 Logging in...')
      const response = await AuthService.login({ email, password })
      
      console.log('✅ Login API successful, saving tokens...')
      // Save tokens using AuthService
      AuthService.saveTokens(response)
      token.value = response.accessToken
      console.log('Token saved:', token.value ? `${token.value.substring(0, 20)}...` : 'null')
      
      // Load user profile after successful login
      console.log('📡 Loading user profile after login...')
      await loadUserProfile()
      
      return { success: true }
    } catch (error: any) {
      console.error('❌ Login error:', error)
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