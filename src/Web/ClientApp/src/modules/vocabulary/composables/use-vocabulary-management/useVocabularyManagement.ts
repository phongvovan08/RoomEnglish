import { ref } from 'vue'
import { createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'

export interface VocabularyStats {
  totalWords: number
  totalExamples: number
  totalCategories: number
  lastUpdated: string
}

export interface RecentActivity {
  id: string
  type: 'import' | 'export' | 'update' | 'delete'
  title: string
  description: string
  date: string
}

export function useVocabularyManagement() {
  const { showSuccess, showError } = useNotifications()

  const vocabularyStats = ref<VocabularyStats>({
    totalWords: 0,
    totalExamples: 0,
    totalCategories: 0,
    lastUpdated: 'Never'
  })

  const recentActivities = ref<RecentActivity[]>([])
  const isLoadingStats = ref(false)
  const isLoadingActivities = ref(false)

  const loadStatistics = async () => {
    isLoadingStats.value = true
    try {
      // TODO: Replace with actual API call
      const response = await fetch('/api/vocabulary/statistics', {
        headers: createAuthHeaders()
      })
      
      if (response.ok) {
        vocabularyStats.value = await response.json()
      } else {
        // Fallback to mock data
        vocabularyStats.value = {
          totalWords: 1250,
          totalExamples: 2840,
          totalCategories: 15,
          lastUpdated: new Date().toLocaleDateString()
        }
      }
    } catch (error) {
      console.error('Failed to load statistics:', error)
      // Use mock data on error
      vocabularyStats.value = {
        totalWords: 0,
        totalExamples: 0,
        totalCategories: 0,
        lastUpdated: 'Never'
      }
    } finally {
      isLoadingStats.value = false
    }
  }

  const loadRecentActivities = async () => {
    isLoadingActivities.value = true
    try {
      // TODO: Replace with actual API call
      const response = await fetch('/api/vocabulary/recent-activities', {
        headers: createAuthHeaders()
      })
      
      if (response.ok) {
        recentActivities.value = await response.json()
      } else {
        // Fallback to mock data
        recentActivities.value = [
          {
            id: '1',
            type: 'import',
            title: 'Excel Import',
            description: '150 words imported successfully',
            date: new Date().toLocaleDateString()
          }
        ]
      }
    } catch (error) {
      console.error('Failed to load recent activities:', error)
      recentActivities.value = []
    } finally {
      isLoadingActivities.value = false
    }
  }

  const uploadExcel = async (file: File) => {
    try {
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch('/api/vocabulary-words/upload-excel', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: formData
      })

      if (!response.ok) {
        throw new Error('Upload failed')
      }

      const result = await response.json()
      showSuccess('Success', `Imported ${result.count || 0} words successfully`)
      
      // Reload statistics
      await loadStatistics()
      await loadRecentActivities()
      
      return result
    } catch (error: any) {
      showError('Upload Failed', error.message || 'Failed to upload Excel file')
      throw error
    }
  }

  const downloadTemplate = async () => {
    try {
      const response = await fetch('/api/vocabulary-words/template.xlsx', {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Download failed')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'VocabularyTemplate.xlsx'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)

      showSuccess('Success', 'Template downloaded successfully')
    } catch (error: any) {
      showError('Download Failed', error.message || 'Failed to download template')
      throw error
    }
  }

  const exportVocabulary = async () => {
    try {
      const response = await fetch('/api/vocabulary/export', {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Export failed')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `Vocabulary_Export_${new Date().toISOString().split('T')[0]}.xlsx`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)

      showSuccess('Success', 'Vocabulary exported successfully')
    } catch (error: any) {
      showError('Export Failed', error.message || 'Failed to export vocabulary')
      throw error
    }
  }

  return {
    // State
    vocabularyStats,
    recentActivities,
    isLoadingStats,
    isLoadingActivities,

    // Methods
    loadStatistics,
    loadRecentActivities,
    uploadExcel,
    downloadTemplate,
    exportVocabulary
  }
}
