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
- ✅ **Phân trang nâng cao** với các tính năng:
  - Navigation buttons (First, Previous, Next, Last)
  - Page numbers với ellipsis (...) cho nhiều trang
  - Input field để nhập trực tiếp số trang
  - Keyboard shortcuts (←→ Home/End)
  - Tùy chọn page size
  - **Server-side và Client-side pagination**
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
| **Server-side Pagination** |
| `serverSide` | `boolean` | `false` | Bật server-side pagination |
| `currentPage` | `number` | `1` | Trang hiện tại (server-side) |
| `totalItems` | `number` | `0` | Tổng số items (server-side) |
| `totalPages` | `number` | `1` | Tổng số trang (server-side) |

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

## Ví dụ sử dụng cơ bản

```vue
<template>
  <DataGrid
    :data="users"
    :columns="userColumns"
    :actions="userActions"
    @row-click="viewUser"
    @action-click="handleAction"
  />
</template>

<script setup lang="ts">
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'

const users = ref([
  { id: 1, name: 'John Doe', email: 'john@example.com', status: 'active' },
  { id: 2, name: 'Jane Smith', email: 'jane@example.com', status: 'inactive' }
])

const userColumns: GridColumn[] = [
  { key: 'name', label: 'Tên', sortable: true },
  { key: 'email', label: 'Email', sortable: true },
  { key: 'status', label: 'Trạng thái' }
]

const userActions: GridAction[] = [
  { key: 'edit', icon: 'mdi:pencil', tooltip: 'Sửa', variant: 'primary' },
  { key: 'delete', icon: 'mdi:delete', tooltip: 'Xóa', variant: 'danger' }
]

const viewUser = (user: any) => {
  console.log('View user:', user)
}

const handleAction = (action: string, user: any) => {
  if (action === 'edit') {
    // Edit logic
  } else if (action === 'delete') {
    // Delete logic
  }
}
</script>
```

## Server-side Pagination

Dành cho datasets lớn khi backend xử lý pagination:

```vue
<template>
  <DataGrid
    :data="items"
    :columns="columns"
    :server-side="true"
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

## Client-side Pagination 

Dành cho datasets nhỏ, tự động phân trang:

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

## VocabularyDataGrid - Component Wrapper

Để sử dụng DataGrid cho từ vựng, chúng ta đã tạo wrapper component `VocabularyDataGrid`:

```vue
<template>
  <VocabularyDataGrid
    :vocabularies="vocabularies"
    :page-size="12"
    @vocabulary-click="viewVocabulary"
    @edit-vocabulary="editVocabulary"
    @delete-vocabulary="deleteVocabulary"
    @create-vocabulary="showCreateModal = true"
    @upload-vocabulary="showUploadModal = true"
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

### Pagination Best Practices

7. **Choose Right Mode**: 
   - `server-side="false"`: Datasets < 1000 items
   - `server-side="true"`: Datasets > 1000 items

8. **Server-side Implementation**:
   ```typescript
   // Backend should return pagination info
   interface ApiResponse {
     items: T[]
     currentPage: number
     totalItems: number  
     totalPages: number
     pageSize: number
   }
   ```

9. **Error Handling**: Handle pagination errors gracefully
   ```vue
   <DataGrid 
     :server-side="true"
     :current-page="currentPage"
     @page-change="handlePageChange"
   />
   
   const handlePageChange = async (page: number) => {
     try {
       await loadData(page)
     } catch (error) {
       // Reset to previous page on error
       console.error('Failed to load page:', error)
     }
   }
   ```

10. **Loading States**: Show loading indicators during page changes

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

**Thư viện được phát triển cho dự án RoomEnglish - Quản lý từ vựng tiếng Anh**