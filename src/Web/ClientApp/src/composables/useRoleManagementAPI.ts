import { useAuthStore } from '@/stores/auth'

const API_BASE = '/api/roles'

// Types
export interface RoleDetail {
  id: string
  name: string
  normalizedName?: string
  description: string
  userCount: number
  createdAt?: string
}

export interface RolesResponse {
  roles: RoleDetail[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

export interface CreateRoleRequest {
  name: string
  description: string
}

export interface UpdateRoleRequest {
  roleId: string
  name: string
  description: string
}

// Helper to create headers with auth token
const createAuthHeaders = () => {
  const authStore = useAuthStore()
  const token = authStore.token

  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` })
  }
}

export const useRoleManagementAPI = () => {
  // Get paginated roles with search
  const getRoles = async (
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string
  ): Promise<RolesResponse> => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    })

    if (searchTerm) {
      params.append('searchTerm', searchTerm)
    }

    const response = await fetch(`${API_BASE}/list?${params}`, {
      headers: createAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch roles')
    }

    return await response.json()
  }

  // Create new role
  const createRole = async (request: CreateRoleRequest): Promise<{ success: boolean; roleId: string }> => {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: createAuthHeaders(),
      body: JSON.stringify(request)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || 'Failed to create role')
    }

    return await response.json()
  }

  // Update role
  const updateRole = async (request: UpdateRoleRequest): Promise<{ success: boolean }> => {
    const response = await fetch(`${API_BASE}/${request.roleId}`, {
      method: 'PUT',
      headers: createAuthHeaders(),
      body: JSON.stringify(request)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || 'Failed to update role')
    }

    return await response.json()
  }

  // Delete role
  const deleteRole = async (roleId: string): Promise<{ success: boolean }> => {
    const response = await fetch(`${API_BASE}/${roleId}`, {
      method: 'DELETE',
      headers: createAuthHeaders()
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || 'Failed to delete role')
    }

    return await response.json()
  }

  return {
    getRoles,
    createRole,
    updateRole,
    deleteRole
  }
}
