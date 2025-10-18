# DataGrid Component Library

Thư viện component DataGrid có thể tái sử dụng cho việc hiển thị dữ liệu dạng bảng và grid.

## Tổng quan

DataGrid là component Vue 3 với TypeScript hỗ trợ hai chế độ hiển thị:
- **Table View**: Hiển thị dữ liệu dạng bảng truyền thống với cột và hàng
- **Grid View**: Hiển thị dữ liệu dạng card layout linh hoạt

## Tính năng chính

- ✅ Chuyển đổi giữa view Table và Grid
- ✅ Tìm kiếm và lọc dữ liệu
- ✅ Sắp xếp theo cột (sortable columns)
- ✅ **Server-side Pagination mặc định** với các tính năng:
  - Navigation buttons (First, Previous, Next, Last)
  - Page numbers với ellipsis (...) cho nhiều trang
  - Input field để nhập trực tiếp số trang
  - Keyboard shortcuts (←→ Home/End)
  - Tùy chọn page size
  - **Optimized cho large datasets**
- ✅ Custom slot cho cell và grid item
- ✅ Action buttons với variants khác nhau
- ✅ Empty state customizable
- ✅ Responsive design
- ✅ TypeScript support

## Cài đặt và Import

```vue
<script setup lang="ts">
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'
</script>
```

## Interface Types

### GridColumn
```typescript
interface GridColumn {
  key: string          // Tên field trong data object
  label: string        // Tiêu đề cột hiển thị
  sortable?: boolean   // Có thể sắp xếp (default: false)
  type?: 'text' | 'number' | 'date'  // Kiểu dữ liệu
  width?: string       // Chiều rộng cột (CSS)
}
```

### GridAction
```typescript
interface GridAction {
  key: string          // Unique identifier
  icon: string         // Iconify icon name
  tooltip: string      // Tooltip text
  variant?: 'default' | 'primary' | 'danger' | 'success'
}
```

## Props

| Prop | Type | Default | Mô tả |
|------|------|---------|--------|
| `data` | `any[]` | **required** | Mảng dữ liệu hiển thị |
| `columns` | `GridColumn[]` | **required** | Cấu hình các cột |
| `actions` | `GridAction[]` | `[]` | Các action button |
| `pagination` | `boolean` | `true` | Bật/tắt phân trang |
| `searchable` | `boolean` | `true` | Bật/tắt tìm kiếm |
| `clickable` | `boolean` | `false` | Row có thể click |
| `pageSize` | `number` | `10` | Số item mỗi trang |
| `pageSizeOptions` | `number[]` | `[5,10,20,50]` | Tùy chọn page size |
| `searchPlaceholder` | `string` | `'Tìm kiếm...'` | Placeholder search |
| `emptyStateTitle` | `string` | `'Không có dữ liệu'` | Tiêu đề empty state |
| `emptyStateMessage` | `string` | `'Chưa có dữ liệu để hiển thị'` | Thông báo empty state |
| `keyField` | `string` | `'id'` | Field làm key cho v-for |
| **Server-side Pagination (Default)** |
| `serverSide` | `boolean` | `true` | Server-side pagination (mặc định) |
| `currentPage` | `number` | `1` | Trang hiện tại (required cho server-side) |
| `totalItems` | `number` | `0` | Tổng số items (required cho server-side) |
| `totalPages` | `number` | `1` | Tổng số trang (required cho server-side) |

## Events

| Event | Payload | Mô tả |
|-------|---------|--------|
| `row-click` | `item: any` | Khi click vào row |
| `action-click` | `action: string, item: any` | Khi click action button |
| `search` | `query: string` | Khi thay đổi search query |
| `page-change` | `page: number` | Khi chuyển trang |
| `page-size-change` | `pageSize: number` | Khi thay đổi page size |
| `sort-change` | `sortBy: string, sortOrder: 'asc'\|'desc'` | Khi sort |

## Slots

### Default Slots
- `cell-{columnKey}`: Custom cell content cho table view
- `grid-item`: Custom grid item layout cho grid view  
- `empty-state`: Custom empty state content

### Slot Props
```vue
<!-- Cell slot -->
<template #cell-status="{ item, value, column }">
  <span :class="getStatusClass(value)">{{ value }}</span>
</template>

<!-- Grid item slot -->
<template #grid-item="{ item, index }">
  <div class="custom-card">{{ item.title }}</div>
</template>

<!-- Empty state slot -->
<template #empty-state>
  <div class="custom-empty">No data found</div>
</template>
```

## Ví dụ sử dụng cơ bản (Server-side)

```vue
<template>
  <DataGrid
    :data="users"
    :columns="userColumns"
    :actions="userActions"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    @row-click="viewUser"
    @action-click="handleAction"
    @page-change="loadPage"
    @search="handleSearch"
  />
</template>

<script setup lang="ts">
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'

// Reactive data for server-side pagination
const currentPage = ref(1)
const totalItems = ref(0)
const totalPages = ref(1)
const users = ref([])

const userColumns: GridColumn[] = [
  { key: 'name', label: 'Tên', sortable: true },
  { key: 'email', label: 'Email', sortable: true },
  { key: 'status', label: 'Trạng thái' }
]

const userActions: GridAction[] = [
  { key: 'edit', icon: 'mdi:pencil', tooltip: 'Sửa', variant: 'primary' },
  { key: 'delete', icon: 'mdi:delete', tooltip: 'Xóa', variant: 'danger' }
]

// Server-side pagination handlers
const loadPage = async (page: number) => {
  const response = await api.get(`/users?page=${page}&size=10`)
  users.value = response.data.items
  currentPage.value = response.data.currentPage
  totalItems.value = response.data.totalItems
  totalPages.value = response.data.totalPages
}

const handleSearch = async (query: string) => {
  const response = await api.get(`/users?page=1&size=10&search=${query}`)
  users.value = response.data.items
  currentPage.value = 1
  totalItems.value = response.data.totalItems
  totalPages.value = response.data.totalPages
}

const viewUser = (user: any) => {
  console.log('View user:', user)
}

const handleAction = (action: string, user: any) => {
  if (action === 'edit') {
    // Edit logic
  } else if (action === 'delete') {
    // Delete logic and reload current page
    await deleteUser(user.id)
    await loadPage(currentPage.value)
  }
}

// Load initial data
onMounted(() => {
  loadPage(1)
})
</script>
```

## Server-side Pagination (Recommended)

**Mặc định và được khuyên dùng cho tất cả cases:**

```vue
<template>
  <DataGrid
    :data="items"
    :columns="columns"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    @page-change="loadPage"
    @page-size-change="changePageSize"
  />
</template>

<script setup>
const currentPage = ref(1)
const totalItems = ref(0)
const totalPages = ref(1)
const items = ref([])

const loadPage = async (page) => {
  const response = await api.get(`/items?page=${page}&size=10`)
  items.value = response.data.items
  currentPage.value = response.data.currentPage
  totalItems.value = response.data.totalItems
  totalPages.value = response.data.totalPages
}
</script>
```

## Client-side Pagination (Legacy)

**Chỉ dùng khi thực sự cần thiết:**

```vue
<template>
  <DataGrid
    :data="allItems"
    :columns="columns"
    :server-side="false"
    :page-size="10"
  />
</template>
```

## Ví dụ với Custom Slots

```vue
<template>
  <DataGrid
    :data="products"
    :columns="productColumns"
    :actions="productActions"
  >
    <!-- Custom status cell -->
    <template #cell-status="{ value }">
      <span :class="['status-badge', \`status-\${value}\`]">
        {{ value.toUpperCase() }}
      </span>
    </template>

    <!-- Custom price cell -->
    <template #cell-price="{ value }">
      <span class="price">{{ formatCurrency(value) }}</span>
    </template>

    <!-- Custom grid item -->
    <template #grid-item="{ item }">
      <div class="product-card">
        <img :src="item.image" :alt="item.name" />
        <h3>{{ item.name }}</h3>
        <p class="price">{{ formatCurrency(item.price) }}</p>
        <span :class="['status', item.status]">{{ item.status }}</span>
      </div>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="no-products">
        <Icon icon="mdi:package-variant" class="w-16 h-16" />
        <h3>Chưa có sản phẩm</h3>
        <button @click="addProduct" class="btn-add">Thêm sản phẩm đầu tiên</button>
      </div>
    </template>
  </DataGrid>
</template>
```

## Wrapper Components - Server-side Ready

### VocabularyDataGrid

Wrapper component cho quản lý từ vựng với server-side pagination:

```vue
<template>
  <VocabularyDataGrid
    :vocabularies="vocabularies"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    :page-size="12"
    @vocabulary-click="viewVocabulary"
    @edit-vocabulary="editVocabulary"
    @delete-vocabulary="deleteVocabulary"
    @create-vocabulary="showCreateModal = true"
    @upload-vocabulary="showUploadModal = true"
    @page-change="loadVocabulariesPage"
    @search="handleVocabularySearch"
  />
</template>
```

### CategoryDataGrid

Wrapper component cho quản lý danh mục với server-side pagination:

```vue
<template>
  <CategoryDataGrid
    :categories="categories"
    :current-page="currentPage"
    :total-items="totalItems" 
    :total-pages="totalPages"
    :page-size="10"
    @category-click="viewCategory"
    @edit-category="editCategory"
    @delete-category="deleteCategory"
    @create-category="showCreateModal = true"
    @page-change="loadCategoriesPage"
    @search="handleCategorySearch"
  />
</template>
```

### VocabularyDataGrid Props
- `vocabularies`: Array các từ vựng
- `page-size`: Số từ vựng mỗi trang
- `current-page`: Trang hiện tại (server-side)
- `total-items`: Tổng số từ vựng (server-side)
- `total-pages`: Tổng số trang (server-side)

### VocabularyDataGrid Events  
- `vocabulary-click`: Khi click vào từ vựng
- `edit-vocabulary`: Khi click sửa
- `delete-vocabulary`: Khi click xóa
- `create-vocabulary`: Khi click thêm mới
- `upload-vocabulary`: Khi click upload
- `search`: Khi search từ vựng
- `page-change`: Khi chuyển trang
- `page-size-change`: Khi thay đổi page size

## Style Customization

DataGrid sử dụng CSS variables và scoped styles. Bạn có thể override styles:

```vue
<style>
.data-grid {
  --grid-bg: #f8f9fa;
  --grid-border: #e9ecef;
  --primary-color: #007bff;
}
</style>
```

## Keyboard Shortcuts

DataGrid hỗ trợ các phím tắt để điều hướng trang nhanh chóng:

| Phím | Chức năng |
|------|-----------|
| `←` (Left Arrow) | Trang trước |
| `→` (Right Arrow) | Trang sau |
| `Home` | Trang đầu tiên |
| `End` | Trang cuối cùng |
| `Enter` (trong input) | Nhảy đến trang đã nhập |

**Lưu ý**: Phím tắt chỉ hoạt động khi không focus vào input field nào khác.

## Best Practices

1. **Performance**: Sử dụng `keyField` phù hợp cho `v-for` optimization
2. **Responsive**: Test trên mobile devices, grid view tự động responsive
3. **Accessibility**: Thêm proper labels và ARIA attributes
4. **Type Safety**: Sử dụng TypeScript interfaces cho data structure
5. **Memory**: Implement virtual scrolling cho large datasets (future enhancement)
6. **User Experience**: Sử dụng keyboard shortcuts cho power users

### Server-side Pagination Best Practices

7. **Use Server-side by Default**: 
   - ✅ **All new components**: Always use server-side pagination
   - ✅ **Consistent performance**: Works with any dataset size
   - ✅ **Future-proof**: Easy to scale

8. **Required Backend API Format**:
   ```typescript
   // API Response format
   interface ApiResponse<T> {
     items: T[]              // Current page items
     currentPage: number     // Current page number (1-based)
     totalItems: number      // Total count of all items
     totalPages: number      // Total number of pages
     pageSize: number        // Items per page
   }
   
   // API Request parameters
   interface ApiRequest {
     page: number           // Page number (1-based)
     size: number          // Page size (10, 20, 50, etc.)
     search?: string       // Search query (optional)
     sortBy?: string       // Sort field (optional)
     sortOrder?: 'asc'|'desc'  // Sort order (optional)
   }
   ```

9. **Error Handling Pattern**:
   ```vue
   <DataGrid 
     :current-page="currentPage"
     :total-items="totalItems"
     :total-pages="totalPages"
     @page-change="handlePageChange"
   />
   
   const handlePageChange = async (page: number) => {
     try {
       await loadData(page)
     } catch (error) {
       // Keep previous page on error
       console.error('Failed to load page:', error)
       showError('Không thể tải dữ liệu. Vui lòng thử lại.')
     }
   }
   ```

10. **Loading States & UX**:
    ```vue
    <DataGrid 
      :data="isLoading ? [] : items"
      :current-page="currentPage"
      @page-change="handlePageChange"
    />
    
    <div v-if="isLoading" class="loading-overlay">
      Đang tải dữ liệu...
    </div>
    ```

11. **Wrapper Component Pattern**:
    - ✅ Create wrapper components for each data type
    - ✅ Handle API calls in wrapper, not in DataGrid
    - ✅ Provide custom slots for domain-specific layouts
    - ✅ Follow naming convention: `{EntityName}DataGrid.vue`

## Pagination Features

### Dual Pagination Modes

**Client-side Pagination** (mặc định):
- Tự động phân trang dữ liệu đã load
- Phù hợp cho datasets nhỏ (< 1000 items)
- Filtering và sorting instant

**Server-side Pagination**:
- Backend xử lý pagination, DataGrid chỉ hiển thị
- Phù hợp cho datasets lớn (> 1000 items) 
- Hiệu suất cao, chỉ load data cần thiết

### Smart Page Numbers
DataGrid tự động hiển thị số trang thông minh với ellipsis:

```
Ít trang:     [1] [2] [3] [4] [5]
Nhiều trang:  [1] [...] [8] [9] [10] [...] [50]
```

### Jump to Page
Người dùng có thể nhập trực tiếp số trang và nhấn Enter để chuyển nhanh.

### Mobile Responsive
Trên mobile, pagination tự động ẩn bớt controls để tiết kiệm không gian.

### Troubleshooting Pagination

**Vấn đề**: Pagination không hiển thị mặc dù có nhiều data
**Nguyên nhân**: Conflict giữa client-side và server-side pagination
**Giải pháp**: Set `server-side="true"` và provide pagination props khi dùng API có sẵn pagination

## Roadmap

- [x] Smart pagination với page numbers
- [x] Jump to page functionality  
- [x] Keyboard navigation
- [x] **Server-side pagination support**
- [x] **Pagination troubleshooting và dual-mode**
- [ ] Virtual scrolling cho large datasets
- [ ] Column resizing
- [ ] Advanced filtering
- [ ] Export functionality (CSV, Excel)
- [ ] Drag & drop row ordering
- [ ] Multi-select rows
- [ ] Inline editing

---

## Creating New Wrapper Components

Use the `_DataGridTemplate.vue` file as a starting point for creating new wrapper components:

### Quick Start with Template

1. **Copy the template**:
   ```bash
   cp _DataGridTemplate.vue UserDataGrid.vue
   ```

2. **Replace placeholders**:
   - Replace `Entity` with your entity name (e.g., `User`, `Product`)
   - Replace `entity` with lowercase entity name (e.g., `user`, `product`)
   - Update interface properties to match your entity
   - Replace `{entity display name}` with proper Vietnamese display name

3. **Customize columns**:
   ```typescript
   const columns = computed<GridColumn[]>(() => [
     {
       key: 'name',
       label: 'Tên người dùng',
       sortable: true,
       type: 'text'
     },
     {
       key: 'email',
       label: 'Email',
       sortable: true,
       type: 'email'
     }
   ])
   ```

4. **Update interface**:
   ```typescript
   interface User {
     id: number
     name: string
     email: string
     createdAt: string
   }
   ```

5. **Customize grid card layout**:
   ```vue
   <template #grid-item="{ item }">
     <div class="user-card">
       <h3>{{ item.name }}</h3>
       <p>{{ item.email }}</p>
     </div>
   </template>
   ```

### Template Features

The `_DataGridTemplate.vue` includes:
- ✅ Server-side pagination (default)
- ✅ Complete TypeScript interfaces
- ✅ Custom grid and table layouts
- ✅ Event handling patterns
- ✅ Empty state management
- ✅ Action buttons and tooltips
- ✅ Responsive design
- ✅ Comprehensive styling

**Thư viện được phát triển cho dự án RoomEnglish - Quản lý từ vựng tiếng Anh**