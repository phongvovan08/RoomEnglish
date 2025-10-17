<template>
  <div class="vocabulary-management">
    <div class="management-header">
      <h1>
        <i class="mdi mdi-book-open-variant"></i>
        Vocabulary Management
      </h1>
      <p>Manage your vocabulary database with bulk import and export features</p>
    </div>

    <div class="management-actions">
      <button @click="showUploadModal = true" class="action-btn primary">
        <i class="mdi mdi-upload"></i>
        Import from Excel
      </button>
      <button @click="downloadTemplate" class="action-btn info">
        <i class="mdi mdi-file-download"></i>
        Download Template
      </button>
      <button @click="exportVocabulary" class="action-btn secondary">
        <i class="mdi mdi-download"></i>
        Export to Excel
      </button>
      <button @click="$router.push('/vocabulary')" class="action-btn tertiary">
        <i class="mdi mdi-arrow-left"></i>
        Back to Learning
      </button>
    </div>

    <div class="statistics-grid">
      <div class="stat-card">
        <div class="stat-icon">
          <i class="mdi mdi-book"></i>
        </div>
        <div class="stat-content">
          <h3>{{ vocabularyStats.totalWords }}</h3>
          <p>Total Words</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="mdi mdi-format-quote-close"></i>
        </div>
        <div class="stat-content">
          <h3>{{ vocabularyStats.totalExamples }}</h3>
          <p>Total Examples</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="mdi mdi-tag"></i>
        </div>
        <div class="stat-content">
          <h3>{{ vocabularyStats.totalCategories }}</h3>
          <p>Categories</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="mdi mdi-clock"></i>
        </div>
        <div class="stat-content">
          <h3>{{ vocabularyStats.lastUpdated }}</h3>
          <p>Last Updated</p>
        </div>
      </div>
    </div>

    <!-- Excel Format Instructions -->
    <div class="instructions-section">
      <h2>
        <i class="mdi mdi-information"></i>
        Excel File Format Instructions
      </h2>
      <div class="instruction-card">
        <div class="instruction-header">
          <i class="mdi mdi-file-excel text-green"></i>
          <h3>Required Excel Columns</h3>
        </div>
        <div class="columns-grid">
          <div class="column-item">
            <span class="column-name">Word</span>
            <span class="column-desc">The vocabulary word (required)</span>
          </div>
          <div class="column-item">
            <span class="column-name">Definition</span>
            <span class="column-desc">Word meaning/definition (required)</span>
          </div>
          <div class="column-item">
            <span class="column-name">Pronunciation</span>
            <span class="column-desc">IPA pronunciation (optional)</span>
          </div>
          <div class="column-item">
            <span class="column-name">CategoryName</span>
            <span class="column-desc">Category name (optional)</span>
          </div>
          <div class="column-item">
            <span class="column-name">Example_Sentence</span>
            <span class="column-desc">Example usage (optional)</span>
          </div>
          <div class="column-item">
            <span class="column-name">Example_Translation</span>
            <span class="column-desc">Vietnamese translation (optional)</span>
          </div>
          <div class="column-item">
            <span class="column-name">Example_Grammar</span>
            <span class="column-desc">Grammar explanation (optional)</span>
          </div>
        </div>
        <div class="instruction-footer">
          <p><i class="mdi mdi-lightbulb text-yellow"></i> 
            <strong>Tip:</strong> Download the template above to see the exact format with examples!</p>
        </div>
      </div>
    </div>

    <!-- Recent Activities -->
    <div class="recent-activities" v-if="recentActivities.length">
      <h2>
        <i class="mdi mdi-history"></i>
        Recent Activities
      </h2>
      <div class="activities-list">
        <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
          <div class="activity-icon" :class="activity.type">
            <i class="mdi" :class="getActivityIcon(activity.type)"></i>
          </div>
          <div class="activity-content">
            <h4>{{ activity.title }}</h4>
            <p>{{ activity.description }}</p>
            <small>{{ formatDate(activity.date) }}</small>
          </div>
        </div>
      </div>
    </div>

    <!-- Upload Modal -->
    <div v-if="showUploadModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <ExcelUploadModal 
          @close="closeModal" 
          @success="handleUploadSuccess"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import ExcelUploadModal from '../components/ExcelUploadModal.vue'
import { createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'

const router = useRouter()
const { showSuccess, showError } = useNotifications()
const showUploadModal = ref(false)

interface VocabularyStats {
  totalWords: number
  totalExamples: number
  totalCategories: number
  lastUpdated: string
}

interface RecentActivity {
  id: string
  type: 'import' | 'export' | 'update' | 'delete'
  title: string
  description: string
  date: string
}

const vocabularyStats = ref<VocabularyStats>({
  totalWords: 0,
  totalExamples: 0,
  totalCategories: 0,
  lastUpdated: 'Never'
})

const recentActivities = ref<RecentActivity[]>([])

onMounted(async () => {
  await loadStatistics()
  await loadRecentActivities()
})

const loadStatistics = async () => {
  try {
    // This would be replaced with actual API calls
    vocabularyStats.value = {
      totalWords: 1250,
      totalExamples: 2840,
      totalCategories: 15,
      lastUpdated: new Date().toLocaleDateString()
    }
  } catch (error) {
    console.error('Error loading statistics:', error)
  }
}

const loadRecentActivities = async () => {
  try {
    // This would be replaced with actual API calls
    recentActivities.value = [
      {
        id: '1',
        type: 'import',
        title: 'Excel Import Completed',
        description: 'Successfully imported 50 words and 120 examples',
        date: new Date().toISOString()
      },
      {
        id: '2',
        type: 'export',
        title: 'Vocabulary Export',
        description: 'Exported complete vocabulary database',
        date: new Date(Date.now() - 86400000).toISOString()
      }
    ]
  } catch (error) {
    console.error('Error loading activities:', error)
  }
}

const exportVocabulary = async () => {
  try {
    const response = await fetch('/api/vocabulary-learning/export-excel', {
      method: 'GET',
      headers: createAuthHeaders()
    })

    if (response.ok) {
      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `vocabulary-export-${new Date().toISOString().split('T')[0]}.xlsx`
      link.click()
      window.URL.revokeObjectURL(url)
      showSuccess('Xuất file Excel thành công!')
    } else {
      showError('Xuất file Excel thất bại: ' + response.statusText)
    }
  } catch (error) {
    console.error('Failed to export vocabulary:', error)
    showError('Lỗi kết nối khi xuất file Excel')
  }
}

const downloadTemplate = async () => {
  const XLSX = await import('xlsx')
  
  // Create a sample Excel data structure
  const templateData = [
    {
      'Word': 'hello',
      'Definition': 'A greeting used when meeting someone',
      'Pronunciation': '/həˈloʊ/',
      'CategoryName': 'Greetings',
      'Example_Sentence': 'Hello, how are you today?',
      'Example_Translation': 'Xin chào, hôm nay bạn khỏe không?',
      'Example_Grammar': 'Simple present tense greeting'
    },
    {
      'Word': 'beautiful',
      'Definition': 'Pleasing to the senses or mind aesthetically',
      'Pronunciation': '/ˈbjuː.t̬ɪ.fəl/',
      'CategoryName': 'Adjectives',
      'Example_Sentence': 'She has a beautiful smile.',
      'Example_Translation': 'Cô ấy có nụ cười xinh đẹp.',
      'Example_Grammar': 'Adjective describing appearance'
    },
    {
      'Word': 'study',
      'Definition': 'To learn about something by reading, memorizing facts',
      'Pronunciation': '/ˈstʌd.i/',
      'CategoryName': 'Education',
      'Example_Sentence': 'I study English every day.',
      'Example_Translation': 'Tôi học tiếng Anh mỗi ngày.',
      'Example_Grammar': 'Simple present tense with frequency adverb'
    },
    {
      'Word': 'computer',
      'Definition': 'An electronic device for storing and processing data',
      'Pronunciation': '/kəmˈpjuː.tər/',
      'CategoryName': 'Technology',
      'Example_Sentence': 'I use my computer for work.',
      'Example_Translation': 'Tôi sử dụng máy tính để làm việc.',
      'Example_Grammar': 'Present simple with object'
    },
    {
      'Word': 'teacher',
      'Definition': 'A person who teaches, especially in a school',
      'Pronunciation': '/ˈtiː.tʃər/',
      'CategoryName': 'Professions',
      'Example_Sentence': 'My teacher is very kind.',
      'Example_Translation': 'Giáo viên của tôi rất tử tế.',
      'Example_Grammar': 'Possessive adjective + noun'
    },
    {
      'Word': 'happy',
      'Definition': 'Feeling or showing pleasure or contentment',
      'Pronunciation': '/ˈhæp.i/',
      'CategoryName': 'Emotions',
      'Example_Sentence': 'I am happy to see you.',
      'Example_Translation': 'Tôi vui mừng được gặp bạn.',
      'Example_Grammar': 'Adjective with linking verb'
    },
    {
      'Word': 'library',
      'Definition': 'A building or room containing collections of books',
      'Pronunciation': '/ˈlaɪ.brer.i/',
      'CategoryName': 'Places',
      'Example_Sentence': 'I borrow books from the library.',
      'Example_Translation': 'Tôi mượn sách từ thư viện.',
      'Example_Grammar': 'Present simple with prepositional phrase'
    }
  ]

  // Create workbook and worksheet
  const workbook = XLSX.utils.book_new()
  const worksheet = XLSX.utils.json_to_sheet(templateData)

  // Set column widths for better readability
  const columnWidths = [
    { wch: 15 }, // Word
    { wch: 50 }, // Definition  
    { wch: 15 }, // Pronunciation
    { wch: 15 }, // CategoryName
    { wch: 40 }, // Example_Sentence
    { wch: 40 }, // Example_Translation
    { wch: 35 }  // Example_Grammar
  ]
  worksheet['!cols'] = columnWidths

  // Add worksheet to workbook
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Vocabulary Template')

  // Generate Excel file and download
  const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' })
  const blob = new Blob([excelBuffer], { 
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
  })
  
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = 'vocabulary-template-example.xlsx'
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

const closeModal = () => {
  showUploadModal.value = false
}

const handleUploadSuccess = (result: any) => {
  console.log('Upload successful:', result)
  // Refresh statistics
  loadStatistics()
  loadRecentActivities()
  // Optionally close modal
  closeModal()
}

const getActivityIcon = (type: string): string => {
  const icons = {
    import: 'mdi-upload',
    export: 'mdi-download',
    update: 'mdi-pencil',
    delete: 'mdi-delete'
  }
  return icons[type as keyof typeof icons] || 'mdi-information'
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleString()
}
</script>

<style scoped>
.vocabulary-management {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  min-height: 100vh;
}

.management-header {
  text-align: center;
  margin-bottom: 3rem;
}

.management-header h1 {
  color: #74c0fc;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  font-size: 2.5rem;
}

.management-header p {
  color: rgba(255, 255, 255, 0.7);
  font-size: 1.1rem;
}

.management-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 3rem;
  flex-wrap: wrap;
}

.action-btn {
  padding: 1rem 2rem;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  text-decoration: none;
}

.action-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
}

.action-btn.primary {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
}

.action-btn.primary:hover {
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.4);
}

.action-btn.secondary {
  background: linear-gradient(135deg, #51cf66, #40c057);
  color: white;
}

.action-btn.secondary:hover {
  box-shadow: 0 8px 25px rgba(81, 207, 102, 0.4);
}

.action-btn.info {
  background: linear-gradient(135deg, #ffd43b, #fab005);
  color: #212529;
  font-weight: 600;
}

.action-btn.info:hover {
  box-shadow: 0 8px 25px rgba(255, 212, 59, 0.4);
  transform: translateY(-2px);
}

.action-btn.tertiary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.action-btn.tertiary:hover {
  background: rgba(255, 255, 255, 0.2);
}

.statistics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-bottom: 3rem;
}

.stat-card {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 2rem;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  gap: 1.5rem;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  background: rgba(255, 255, 255, 0.08);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  font-size: 1.8rem;
}

.stat-content h3 {
  color: white;
  font-size: 2rem;
  margin: 0 0 0.5rem 0;
}

.stat-content p {
  color: rgba(255, 255, 255, 0.7);
  margin: 0;
  font-size: 0.9rem;
}

.recent-activities {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 20px;
  padding: 2rem;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.recent-activities h2 {
  color: #74c0fc;
  margin-bottom: 2rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.activities-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.activity-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.5rem;
}

.activity-icon.import {
  background: linear-gradient(135deg, #74c0fc, #339af0);
}

.activity-icon.export {
  background: linear-gradient(135deg, #51cf66, #40c057);
}

.activity-icon.update {
  background: linear-gradient(135deg, #ffd43b, #fab005);
}

.activity-icon.delete {
  background: linear-gradient(135deg, #ff6b6b, #fa5252);
}

.activity-content h4 {
  color: white;
  margin: 0 0 0.25rem 0;
}

.activity-content p {
  color: rgba(255, 255, 255, 0.7);
  margin: 0 0 0.25rem 0;
}

.activity-content small {
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.8rem;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(5px);
}

.modal-content {
  max-width: 90vw;
  max-height: 90vh;
  overflow-y: auto;
  border-radius: 20px;
}

/* Instructions Section */
.instructions-section {
  margin: 3rem 0;
}

.instructions-section h2 {
  color: white;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.5rem;
}

.instruction-card {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 2rem;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.instruction-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.instruction-header h3 {
  color: white;
  margin: 0;
  font-size: 1.25rem;
}

.instruction-header .mdi {
  font-size: 2rem;
}

.text-green {
  color: #51cf66;
}

.text-yellow {
  color: #ffd43b;
}

.columns-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.column-item {
  background: rgba(255, 255, 255, 0.05);
  padding: 1rem;
  border-radius: 12px;
  border-left: 4px solid #74c0fc;
  transition: transform 0.2s ease;
}

.column-item:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.1);
}

.column-name {
  display: block;
  color: #74c0fc;
  font-weight: 600;
  font-size: 0.95rem;
  margin-bottom: 0.5rem;
}

.column-desc {
  display: block;
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.85rem;
  line-height: 1.4;
}

.instruction-footer {
  background: rgba(255, 212, 59, 0.1);
  border: 1px solid rgba(255, 212, 59, 0.2);
  border-radius: 12px;
  padding: 1rem;
  color: white;
}

.instruction-footer p {
  margin: 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.instruction-footer .mdi {
  font-size: 1.2rem;
}

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .management-actions {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .action-btn {
    padding: 1rem;
  }
  
  .statistics-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .columns-grid {
    grid-template-columns: 1fr;
  }
  
  .instruction-card {
    padding: 1.5rem;
  }
  
  .instruction-header {
    flex-direction: column;
    text-align: center;
    gap: 0.5rem;
  }
}
</style>