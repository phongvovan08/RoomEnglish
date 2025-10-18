import { AuthService } from '@/services/authService'

/**
 * Get authentication token from AuthService
 * @returns The auth token or null if not found
 */
export const getAuthToken = (): string | null => {
  return AuthService.getToken()
}

/**
 * Create authorization headers with Bearer token.
 * @param additionalHeaders - Optional headers to merge into the result.
 * @param contentType - Content type to include. Pass `null`/`undefined` to skip.
 *                      When sending FormData use `'multipart/form-data'` (or omit)
 *                      so the browser can append the boundary automatically.
 * @returns Headers object containing Authorization and optional Content-Type.
 */
export const createAuthHeaders = (
  additionalHeaders?: Record<string, string>,
  contentType: string | null = 'application/json'
): Record<string, string> => {
  const headers: Record<string, string> = {}
  const normalizedContentType = contentType?.toLowerCase()

  // Skip multipart so the fetch/XHR client can add the boundary automatically.
  if (contentType && !(normalizedContentType && normalizedContentType.includes('multipart/form-data'))) {
    headers['Content-Type'] = contentType
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
 * Create headers specifically for file uploads (FormData)
 * Only includes Authorization header, lets browser handle Content-Type with boundary
 * @returns Headers object with only Authorization header (if token exists)
 */
export const createFileUploadHeaders = (): Record<string, string> => {
  const headers: Record<string, string> = {}
  
  const token = getAuthToken()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
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
        if (token) {
          localStorage.setItem('access_token', token) // Fallback to direct localStorage
        }
      }
    } catch (error) {
      console.error('Failed to get default token:', error)
    }
  }
  
  return token
}