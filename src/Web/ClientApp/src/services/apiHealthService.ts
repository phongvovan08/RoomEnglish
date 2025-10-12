import appConfig from '@/config/app.config'

export class ApiHealthService {
  private static readonly HEALTH_URL = `${appConfig.apiBaseUrl}/health`
  
  // Check if API server is reachable
  static async checkApiHealth(): Promise<{ 
    isHealthy: boolean
    message: string
    status?: number 
  }> {
    try {
      const response = await fetch(this.HEALTH_URL, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        // Short timeout for health check
        signal: AbortSignal.timeout(5000)
      })

      if (response.ok) {
        return {
          isHealthy: true,
          message: 'API server is running',
          status: response.status
        }
      } else {
        return {
          isHealthy: false,
          message: `API server responded with status ${response.status}`,
          status: response.status
        }
      }
    } catch (error) {
      if (error instanceof Error) {
        if (error.name === 'TimeoutError') {
          return {
            isHealthy: false,
            message: 'API server timeout - server may be slow or unreachable'
          }
        }
        if (error.message.includes('Failed to fetch')) {
          return {
            isHealthy: false,
            message: 'Cannot connect to API server. Please check if backend is running on https://localhost:5001'
          }
        }
      }
      return {
        isHealthy: false,
        message: 'Unknown API connection error'
      }
    }
  }

  // Enhanced error message based on error type
  static getErrorMessage(error: unknown): string {
    if (error instanceof Error) {
      if (error.message.includes('Failed to fetch')) {
        return 'Cannot connect to API server. Please check:\n• Backend is running on https://localhost:5001\n• CORS is configured correctly\n• No firewall blocking the connection'
      }
      if (error.message.includes('401')) {
        return 'Authentication failed. Please login again.'
      }
      if (error.message.includes('403')) {
        return 'Access denied. You do not have permission for this action.'
      }
      if (error.message.includes('404')) {
        return 'API endpoint not found. The backend may be misconfigured.'
      }
      if (error.message.includes('500')) {
        return 'Internal server error. Please check the backend logs.'
      }
      return error.message
    }
    return 'Unknown error occurred'
  }
}

// Composable for API health checking
export function useApiHealth() {
  const isHealthy = ref<boolean | null>(null)
  const healthMessage = ref<string>('')
  const checking = ref(false)

  const checkHealth = async (): Promise<boolean> => {
    checking.value = true
    try {
      const result = await ApiHealthService.checkApiHealth()
      isHealthy.value = result.isHealthy
      healthMessage.value = result.message
      return result.isHealthy
    } finally {
      checking.value = false
    }
  }

  return {
    isHealthy: readonly(isHealthy),
    healthMessage: readonly(healthMessage),
    checking: readonly(checking),
    checkHealth
  }
}