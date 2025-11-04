export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  confirmPassword?: string
}

export interface AuthResponse {
  tokenType: string
  accessToken: string
  expiresIn: number
  refreshToken: string
}

export interface UserInfo {
  email: string
  isEmailConfirmed: boolean
}

// Import app configuration
import appConfig from '@/config/app.config'

export class AuthService {
  private static readonly BASE_URL = appConfig.apiBaseUrl

  static async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${this.BASE_URL}${appConfig.endpoints.auth.login}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Login failed')
    }

    return response.json()
  }

  static async register(userData: RegisterRequest): Promise<void> {
    const response = await fetch(`${this.BASE_URL}${appConfig.endpoints.auth.register}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Registration failed')
    }
  }

  static async getUserInfo(): Promise<UserInfo> {
    const token = this.getToken()
    if (!token) {
      throw new Error('No access token found')
    }

    const response = await fetch(`${this.BASE_URL}${appConfig.endpoints.auth.userInfo}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error('Failed to get user info')
    }

    return response.json()
  }

  static async refreshToken(): Promise<AuthResponse> {
    const refreshToken = this.getRefreshToken()
    if (!refreshToken) {
      throw new Error('No refresh token found')
    }

    const response = await fetch(`${this.BASE_URL}${appConfig.endpoints.auth.refresh}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    })

    if (!response.ok) {
      throw new Error('Token refresh failed')
    }

    return response.json()
  }

  static async logout(): Promise<void> {
    // Clear tokens from storage first
    this.clearTokens()
    
    // MapIdentityApi doesn't provide a logout endpoint
    // Tokens are stateless, so clearing client-side is sufficient
    console.log('✅ Logged out successfully (client-side)')
  }

  // Token management
  static saveTokens(authResponse: AuthResponse): void {
    localStorage.setItem(appConfig.auth.tokenKey, authResponse.accessToken)
    localStorage.setItem(appConfig.auth.refreshTokenKey, authResponse.refreshToken)
    localStorage.setItem(appConfig.auth.tokenExpiryKey, 
      (Date.now() + authResponse.expiresIn * 1000).toString()
    )
  }

  static getToken(): string | null {
    return localStorage.getItem(appConfig.auth.tokenKey)
  }

  static getRefreshToken(): string | null {
    return localStorage.getItem(appConfig.auth.refreshTokenKey)
  }

  static isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem(appConfig.auth.tokenExpiryKey)
    if (!expiresAt) return true
    return Date.now() > parseInt(expiresAt)
  }

  static clearTokens(): void {
    localStorage.removeItem(appConfig.auth.tokenKey)
    localStorage.removeItem(appConfig.auth.refreshTokenKey) 
    localStorage.removeItem(appConfig.auth.tokenExpiryKey)
  }

  static isAuthenticated(): boolean {
    const token = this.getToken()
    return !!token && !this.isTokenExpired()
  }
}