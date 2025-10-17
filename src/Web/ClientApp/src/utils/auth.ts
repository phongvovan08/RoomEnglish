import { AuthService } from '@/services/authService'

/**
 * Get authentication token from AuthService
 * @returns The auth token or null if not found
 */
export const getAuthToken = (): string | null => {
  return AuthService.getToken()
}

/**
 * Create authorization headers with Bearer token
 * @param additionalHeaders - Additional headers to merge (optional)
 * @param contentType - Content type to use, defaults to 'application/json'
 * @returns Headers object with Authorization and Content-Type
 * headers: createAuthHeaders()
// -> { 'Content-Type': 'application/json', 'Authorization': 'Bearer ...' }
headers: createAuthHeaders({}, 'multipart/form-data')
// -> { 'Content-Type': 'multipart/form-data', 'Authorization': 'Bearer ...' }
headers: createAuthHeaders({ 'Accept': 'application/xml', 'X-Custom': 'value' })
// -> { 'Content-Type': 'application/json', 'Authorization': 'Bearer ...', 'Accept': 'application/xml', 'X-Custom': 'value' }
headers: createAuthHeaders({ 'Accept': 'text/plain' }, 'text/plain')
// -> { 'Content-Type': 'text/plain', 'Authorization': 'Bearer ...', 'Accept': 'text/plain' }
 */
export const createAuthHeaders = (
  additionalHeaders?: Record<string, string>,
  contentType: string = 'application/json'
): Record<string, string> => {
  const headers: Record<string, string> = {
    'Content-Type': contentType
  }
  
  const token = getAuthToken()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  
  // Merge additional headers (they can override defaults)
  if (additionalHeaders) {
    Object.assign(headers, additionalHeaders)
  }
  
  return headers
}

/**
 * Get auth token with fallback to default token endpoint
 * @returns Promise<string | null>
 */
export const getAuthTokenWithFallback = async (): Promise<string | null> => {
  let token = getAuthToken()
  
  // If no token, try to get one from the default endpoint
  if (!token) {
    try {
      const tokenResponse = await fetch('/api/Authentication/GetDefaultToken', {
        method: 'POST'
      })
      if (tokenResponse.ok) {
        token = await tokenResponse.text()
        // Save token using AuthService method if available
        localStorage.setItem('access_token', token) // Fallback to direct localStorage
      }
    } catch (error) {
      console.error('Failed to get default token:', error)
    }
  }
  
  return token
}