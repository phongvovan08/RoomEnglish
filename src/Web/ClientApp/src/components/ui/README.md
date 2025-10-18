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
- ✅ Phân trang với tùy chọn page size
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

### VocabularyDataGrid Events  
- `vocabulary-click`: Khi click vào từ vựng
- `edit-vocabulary`: Khi click sửa
- `delete-vocabulary`: Khi click xóa
- `create-vocabulary`: Khi click thêm mới
- `upload-vocabulary`: Khi click upload

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

## Best Practices

1. **Performance**: Sử dụng `keyField` phù hợp cho `v-for` optimization
2. **Responsive**: Test trên mobile devices, grid view tự động responsive
3. **Accessibility**: Thêm proper labels và ARIA attributes
4. **Type Safety**: Sử dụng TypeScript interfaces cho data structure
5. **Memory**: Implement virtual scrolling cho large datasets (future enhancement)

## Roadmap

- [ ] Virtual scrolling cho large datasets
- [ ] Column resizing
- [ ] Advanced filtering
- [ ] Export functionality (CSV, Excel)
- [ ] Drag & drop row ordering
- [ ] Multi-select rows
- [ ] Inline editing

---

**Thư viện được phát triển cho dự án RoomEnglish - Quản lý từ vựng tiếng Anh**