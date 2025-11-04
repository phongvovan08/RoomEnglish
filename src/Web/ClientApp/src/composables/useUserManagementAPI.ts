import { API_CONFIG } from '@/config/api.config'
import { createAuthHeaders } from '@/utils/auth'

const API_BASE = `${API_CONFIG.baseURL}/api/users`

export interface User {
  id: string
  email: string
  userName?: string
  firstName?: string
  lastName?: string
  emailConfirmed: boolean
  roles: string[]
  createdAt?: string
  lastLoginAt?: string
}

export interface Role {
  name: string
  description: string
}

export interface UsersResponse {
  users: User[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

export interface UpdateRolesRequest {
  userId: string
  roles: string[]
}

export interface CreateUserRequest {
  email: string
  password: string
  userName?: string
  roles?: string[]
}

export interface RoleDto {
  name: string
  description: string
}

export interface AvailableRolesResponse {
  roles: RoleDto[]
}

export const useUserManagementAPI = () => {
  const getUsers = async (
    searchTerm?: string,
    role?: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<UsersResponse | null> => {
    try {
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
      })
      
      if (searchTerm) params.append('searchTerm', searchTerm)
      if (role) params.append('role', role)

      const response = await fetch(`${API_BASE}/list?${params}`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        console.error('Failed to fetch users:', response.status)
        return null
      }

      return await response.json()
    } catch (err: any) {
      console.error('Error fetching users:', err.message)
      return null
    }
  }

  const getAvailableRoles = async (): Promise<RoleDto[]> => {
    try {
      const response = await fetch(`${API_BASE}/roles`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        console.error('Failed to fetch roles:', response.status)
        return []
      }

      const data: AvailableRolesResponse = await response.json()
      console.log('📋 Roles from API:', data.roles)
      return data.roles || []
    } catch (err: any) {
      console.error('Error fetching roles:', err.message)
      return []
    }
  }

  const updateUserRoles = async (userId: string, roles: string[]): Promise<boolean> => {
    try {
      console.log('🔄 Updating user roles:', { userId, roles })
      
      const requestBody = { userId, roles }
      console.log('📤 Request body:', JSON.stringify(requestBody))
      
      const response = await fetch(`${API_BASE}/${userId}/roles`, {
        method: 'PUT',
        headers: {
          ...createAuthHeaders(),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
      })

      console.log('📥 Response status:', response.status)

      if (!response.ok) {
        const errorText = await response.text()
        console.error('❌ Failed to update user roles:', response.status)
        console.error('Error details:', errorText)
        return false
      }

      console.log('✅ User roles updated successfully')
      return true
    } catch (err: any) {
      console.error('💥 Error updating user roles:', err.message)
      return false
    }
  }

  const createUser = async (request: CreateUserRequest): Promise<{ success: boolean; userId?: string }> => {
    try {
      console.log('🔄 Creating user:', request.email)
      
      const response = await fetch(API_BASE, {
        method: 'POST',
        headers: {
          ...createAuthHeaders(),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      })

      if (!response.ok) {
        const error = await response.json()
        throw new Error(error.detail || 'Failed to create user')
      }

      const data = await response.json()
      console.log('✅ User created successfully:', data.userId)
      return { success: true, userId: data.userId }
    } catch (err: any) {
      console.error('❌ Error creating user:', err.message)
      throw err
    }
  }

  const deleteUser = async (userId: string): Promise<boolean> => {
    try {
      console.log('🔄 Deleting user:', userId)
      
      const response = await fetch(`${API_BASE}/${userId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        const error = await response.json()
        throw new Error(error.detail || 'Failed to delete user')
      }

      console.log('✅ User deleted successfully')
      return true
    } catch (err: any) {
      console.error('❌ Error deleting user:', err.message)
      throw err
    }
  }

  return {
    getUsers,
    getAvailableRoles,
    updateUserRoles,
    createUser,
    deleteUser
  }
}
