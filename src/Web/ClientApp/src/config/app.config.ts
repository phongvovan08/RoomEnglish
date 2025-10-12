// Application Configuration
export const appConfig = {
  // API Base URL - read from environment variables
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001',
  
  // Other app settings
  app: {
    name: 'RoomEnglish',
    version: '1.0.0',
  },
  
  // Authentication settings
  auth: {
    tokenKey: 'access_token',
    refreshTokenKey: 'refresh_token',
    tokenExpiryKey: 'token_expires_at',
  },
  
  // API endpoints
  endpoints: {
    auth: {
      login: '/api/Users/login',
      register: '/api/Users/register', 
      refresh: '/api/Users/refresh',
      userInfo: '/api/Users/manage/info',
      logout: '/api/Users/logout',
    }
  }
}

export default appConfig