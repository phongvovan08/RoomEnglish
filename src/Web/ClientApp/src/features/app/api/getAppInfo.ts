import { apiService } from '@/services/api'

export interface AppInfoResponse {
  version: string
  environment: string
}

export const getAppInfo = async (): Promise<AppInfoResponse> => {
  return apiService.get<AppInfoResponse>('/AppInfo/info')
}
