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

export class AuthService {
  private static readonly BASE_URL = 'https://localhost:5001/api/Users'

  static async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${this.BASE_URL}/login`, {
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
    const response = await fetch(`${this.BASE_URL}/register`, {
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

    const response = await fetch(`${this.BASE_URL}/manage/info`, {
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

    const response = await fetch(`${this.BASE_URL}/refresh`, {
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
    // Clear tokens from storage
    this.clearTokens()
    
    // Optionally call logout endpoint if available
    try {
      await fetch(`${this.BASE_URL}/Users/logout`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.getToken()}`,
        },
      })
    } catch (error) {
      // Ignore logout endpoint errors
      console.warn('Logout endpoint failed:', error)
    }
  }

  // Token management
  static saveTokens(authResponse: AuthResponse): void {
    localStorage.setItem('access_token', authResponse.accessToken)
    localStorage.setItem('refresh_token', authResponse.refreshToken)
    localStorage.setItem('token_expires_at', 
      (Date.now() + authResponse.expiresIn * 1000).toString()
    )
  }

  static getToken(): string | null {
    return localStorage.getItem('access_token')
  }

  static getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token')
  }

  static isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem('token_expires_at')
    if (!expiresAt) return true
    return Date.now() > parseInt(expiresAt)
  }

  static clearTokens(): void {
    localStorage.removeItem('access_token')
    localStorage.removeItem('refresh_token') 
    localStorage.removeItem('token_expires_at')
  }

  static isAuthenticated(): boolean {
    const token = this.getToken()
    return !!token && !this.isTokenExpired()
  }
}