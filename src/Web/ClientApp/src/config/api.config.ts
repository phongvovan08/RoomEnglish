export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_URL || '/api',
  timeout: 30000,
} as const

export default API_CONFIG
