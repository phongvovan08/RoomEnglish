# 🔐 Role-Based Menu Visibility

## Tổng quan
Menu "Quản lý" (Management) chỉ hiển thị khi user có quyền **Administrator**.

## Cách hoạt động

### 1. **CyborgMenu.vue - Menu Component**

```typescript
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const menuItems = computed(() => {
  const items = [
    // Home, Vocabulary, etc. - Luôn hiển thị
  ]

  // Chỉ thêm menu "Quản lý" nếu user có role Administrator
  if (authStore.hasRole('Administrator')) {
    items.push({
      name: 'management',
      label: 'Quản lý',
      children: [
        { name: 'manageCategories', label: 'Quản lý Danh mục' },
        { name: 'manageUsers', label: 'Quản lý Tài khoản' },
      ],
    })
  }

  return items
})
```

### 2. **authStore.hasRole() - Kiểm tra quyền**

```typescript
// stores/auth.ts
const hasRole = (role: string) => {
  console.log('🔍 Checking role:', role, 'User roles:', user.value?.roles)
  return user.value?.roles?.includes(role) ?? false
}
```

### 3. **Router Guard - Bảo vệ route**

```typescript
// router/index.ts
router.beforeEach(async (to, from) => {
  if (to.meta.requiresAdmin) {
    const authStore = useAuthStore()
    
    if (!authStore.user) {
      await authStore.loadUserProfile()
    }
    
    if (!authStore.hasRole('Administrator')) {
      return { name: 'AccessDenied' }
    }
  }
})
```

## Kết quả

### User KHÔNG có quyền Administrator:
```
Menu hiển thị:
├── Home
├── Vocabulary Learning
│   ├── Categories
│   ├── Words
│   ├── Examples
│   └── My Progress
└── (Không có menu "Quản lý")
```

### User CÓ quyền Administrator:
```
Menu hiển thị:
├── Home
├── Vocabulary Learning
│   ├── Categories
│   ├── Words
│   ├── Examples
│   └── My Progress
└── Quản lý ✅
    ├── Quản lý Danh mục
    └── Quản lý Tài khoản
```

## Test

### 1. Test với user thường (không có quyền):
```
1. Login với user không có role Administrator
2. Menu "Quản lý" KHÔNG hiển thị
3. Nếu truy cập trực tiếp /management/users → Redirect to Access Denied
```

### 2. Test với Admin:
```
1. Login với devphongvv198@gmail.com (có role Administrator)
2. Menu "Quản lý" HIỂN THỊ
3. Click vào "Quản lý Tài khoản" → Truy cập thành công
```

## Console Logs để debug

Khi menu được render:
```
🔍 Checking role: Administrator User roles: ['Administrator']
→ Menu "Quản lý" được thêm vào
```

Khi user không có quyền:
```
🔍 Checking role: Administrator User roles: []
→ Menu "Quản lý" không được thêm vào
```

## Lưu ý quan trọng

⚠️ **Menu visibility chỉ là UI/UX, KHÔNG phải security!**

Backend PHẢI validate roles:
```csharp
[Authorize(Roles = "Administrator")]
public class GetUsersQuery : IRequest<UsersVm> { }
```

Frontend route guard chỉ là lớp bảo vệ thứ hai, backend là lớp chính!

## Mở rộng

### Thêm menu khác có điều kiện:

```typescript
// Ví dụ: Menu "Reports" chỉ hiện với role "Manager" hoặc "Administrator"
if (authStore.hasRole('Manager') || authStore.hasRole('Administrator')) {
  items.push({
    name: 'reports',
    label: 'Báo cáo',
    children: [...]
  })
}
```

### Ẩn menu item con:

```typescript
{
  name: 'management',
  label: 'Quản lý',
  children: [
    {
      name: 'manageCategories',
      label: 'Quản lý Danh mục',
    },
    // Chỉ hiện "Quản lý Tài khoản" với SuperAdmin
    ...(authStore.hasRole('SuperAdmin') ? [{
      name: 'manageUsers',
      label: 'Quản lý Tài khoản',
    }] : []),
  ],
}
```

## Troubleshooting

### Menu không ẩn sau khi login:
```
Nguyên nhân: Computed property không reactive
Giải pháp: Đảm bảo authStore.user được update sau login
```

### Menu không hiện sau khi có quyền:
```
Nguyên nhân: authStore.user chưa load roles
Giải pháp: 
1. Check console log: "User roles: ['Administrator']"
2. Kiểm tra loadUserProfile() đã được gọi
```

### Menu hiện nhưng vẫn Access Denied:
```
Nguyên nhân: Router guard check khác với menu visibility
Giải pháp: Đảm bảo cùng dùng authStore.hasRole('Administrator')
```

## Files đã thay đổi

1. ✅ `CyborgMenu.vue` - Thêm conditional rendering cho menu Management
2. ✅ `auth.ts` - hasRole() function với debug logs
3. ✅ `router/index.ts` - Admin guard cho management routes
4. ✅ `main.ts` - initializeAuth() để load user profile on startup

## Kết luận

✅ Menu "Quản lý" chỉ hiện với Administrator
✅ Router guard bảo vệ routes
✅ Backend API validation (Authorize attribute)
✅ UI/UX tốt - User không thấy menu không có quyền truy cập

🎯 **Security**: Frontend ẩn menu + Router guard + Backend validation = 3 lớp bảo vệ!
