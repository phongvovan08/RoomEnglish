import { ref } from 'vue'
import { createAuthHeaders } from '@/utils/auth'

interface VocabularyStats {
  totalWords: number
  totalExamples: number
  totalCategories: number
  lastUpdated: string
}

interface RecentActivity {
  id: number
  action: string
  description: string
  timestamp: string
  user: string
}

export function useVocabularyManagement() {
  const vocabularyStats = ref<VocabularyStats>({
    totalWords: 0,
    totalExamples: 0,
    totalCategories: 0,
    lastUpdated: 'Never'
  })

  const recentActivities = ref<RecentActivity[]>([])
  const isLoadingStats = ref(false)
  const isUploading = ref(false)
  const isExporting = ref(false)

  const loadStatistics = async () => {
    isLoadingStats.value = true
    try {
      const response = await fetch('/api/vocabulary-words/statistics', {
        headers: createAuthHeaders()
      })

      if (response.ok) {
        const data = await response.json()
        vocabularyStats.value = {
          totalWords: data.totalWords || 0,
          totalExamples: data.totalExamples || 0,
          totalCategories: data.totalCategories || 0,
          lastUpdated: data.lastUpdated ? new Date(data.lastUpdated).toLocaleDateString() : 'Never'
        }
      }
    } catch (error) {
      console.error('Failed to load statistics:', error)
    } finally {
      isLoadingStats.value = false
    }
  }

  const loadRecentActivities = async () => {
    try {
      const response = await fetch('/api/vocabulary-words/recent-activities', {
        headers: createAuthHeaders()
      })

      if (response.ok) {
        recentActivities.value = await response.json()
      }
    } catch (error) {
      console.error('Failed to load recent activities:', error)
    }
  }

  const uploadExcel = async (file: File): Promise<boolean> => {
    isUploading.value = true
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

      if (response.ok) {
        await loadStatistics()
        await loadRecentActivities()
        return true
      }
      return false
    } catch (error) {
      console.error('Failed to upload Excel:', error)
      return false
    } finally {
      isUploading.value = false
    }
  }

  const downloadTemplate = async () => {
    try {
      const response = await fetch('/api/vocabulary-words/template.xlsx', {
        headers: createAuthHeaders()
      })

      if (response.ok) {
        const blob = await response.blob()
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = 'VocabularyTemplate.xlsx'
        document.body.appendChild(a)
        a.click()
        document.body.removeChild(a)
        window.URL.revokeObjectURL(url)
      }
    } catch (error) {
      console.error('Failed to download template:', error)
    }
  }

  const exportVocabulary = async () => {
    isExporting.value = true
    try {
      const response = await fetch('/api/vocabulary-words/export', {
        headers: createAuthHeaders()
      })

      if (response.ok) {
        const blob = await response.blob()
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `Vocabulary_Export_${new Date().toISOString().split('T')[0]}.xlsx`
        document.body.appendChild(a)
        a.click()
        document.body.removeChild(a)
        window.URL.revokeObjectURL(url)
      }
    } catch (error) {
      console.error('Failed to export vocabulary:', error)
    } finally {
      isExporting.value = false
    }
  }

  return {
    vocabularyStats,
    recentActivities,
    isLoadingStats,
    isUploading,
    isExporting,
    loadStatistics,
    loadRecentActivities,
    uploadExcel,
    downloadTemplate,
    exportVocabulary
  }
}
