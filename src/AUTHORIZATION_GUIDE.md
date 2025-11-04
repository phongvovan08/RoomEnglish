# Authorization - Phân Quyền Truy Cập

## Tổng Quan
Hệ thống phân quyền đã được cấu hình để bảo vệ các trang quản lý (Management). Chỉ những tài khoản có quyền **Administrator** mới có thể truy cập các trang này.

## Cấu Hình Đã Thực Hiện

### 1. Router Meta - Đánh Dấu Route Cần Admin

File: `Web/ClientApp/src/router/index.ts`

```typescript
// Management System routes
{
  path: Routes.Management.children.Categories.path,
  name: Routes.Management.children.Categories.name,
  component: () => import("../modules/management/views/CategoriesManagement.vue"),
  meta: { 
    requiresAuth: true,      // Yêu cầu đăng nhập
    requiresAdmin: true       // Yêu cầu quyền Administrator
  },
},
{
  path: Routes.Management.children.Vocabularies.children.List.path,
  name: Routes.Management.children.Vocabularies.children.List.name,
  component: () => import("../modules/management/views/VocabulariesManagement.vue"),
  meta: { 
    requiresAuth: true,
    requiresAdmin: true 
  },
},
{
  path: Routes.Management.children.Examples.children.List.path,
  name: Routes.Management.children.Examples.children.List.name,
  component: () => import("../modules/management/views/ExamplesManagement.vue"),
  meta: { 
    requiresAuth: true,
    requiresAdmin: true 
  },
},
```

### 2. Navigation Guard - Kiểm Tra Quyền Truy Cập

File: `Web/ClientApp/src/router/index.ts`

```typescript
import { AuthService } from '@/services/authService'
import { useAuthStore } from '@/stores/auth'

router.beforeEach(async (to, from) => {
  // Kiểm tra yêu cầu đăng nhập
  if (to.meta.requiresAuth) {
    if (!AuthService.isAuthenticated()) {
      return {
        name: Routes.Auth.children.Login.name,
        query: { redirect: to.fullPath }
      }
    }

    // Kiểm tra yêu cầu quyền Administrator
    if (to.meta.requiresAdmin) {
      const authStore = useAuthStore()
      
      // Đảm bảo user data đã được load
      if (!authStore.user) {
        await authStore.loadUserProfile()
      }
      
      // Kiểm tra user có role Administrator không
      if (!authStore.hasRole('Administrator')) {
        console.warn('Access denied: User does not have Administrator role')
        return { name: 'AccessDenied' }
      }
    }
  }
})
```

### 3. TypeScript Type Definitions

File: `Web/ClientApp/src/types/router.d.ts`

```typescript
import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean    // Yêu cầu đăng nhập
    requiresAdmin?: boolean   // Yêu cầu quyền Administrator
    public?: boolean          // Route công khai
    guest?: boolean          // Chỉ cho guest (chưa đăng nhập)
  }
}
```

### 4. Auth Store - Kiểm Tra Role

File: `Web/ClientApp/src/stores/auth.ts`

```typescript
interface User {
  id: string
  email: string
  firstName?: string
  lastName?: string
  displayName?: string
  roles: string[]  // Danh sách roles của user
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  
  // Kiểm tra user có role cụ thể không
  const hasRole = (role: string) => user.value?.roles?.includes(role) ?? false
  
  // Load thông tin user từ API
  const loadUserProfile = async () => {
    if (!token.value) return

    const response = await fetch('/api/users/me', {
      headers: {
        'Authorization': `Bearer ${token.value}`,
      }
    })

    if (response.ok) {
      const userData = await response.json()
      setUser(userData)
    }
  }
  
  return {
    hasRole,
    loadUserProfile,
    // ...
  }
})
```

## Backend - Roles

File: `Domain/Constants/Roles.cs`

```csharp
public static class Roles
{
    public const string Administrator = nameof(Administrator);
    // Có thể thêm roles khác
    // public const string User = nameof(User);
    // public const string Moderator = nameof(Moderator);
}
```

## Cách Hoạt Động

### Khi User Truy Cập Route Management

```
1. User click vào "Categories Management"
   ↓
2. Router beforeEach chặn
   ↓
3. Kiểm tra requiresAuth: true
   → Đã đăng nhập? ✅
   ↓
4. Kiểm tra requiresAdmin: true
   ↓
5. Load user profile nếu chưa có
   ↓
6. Kiểm tra hasRole('Administrator')
   ├─ CÓ quyền → Cho phép truy cập ✅
   └─ KHÔNG có quyền → Redirect đến AccessDenied ❌
```

### Luồng Chi Tiết

**User KHÔNG có quyền Admin:**
```
URL: /management/categories
  ↓
beforeEach hook
  ↓
requiresAuth: true → isAuthenticated() = true ✅
  ↓
requiresAdmin: true → hasRole('Administrator') = false ❌
  ↓
Redirect to: /access-denied
  ↓
Hiển thị: "Bạn không có quyền truy cập trang này"
```

**User CÓ quyền Admin:**
```
URL: /management/categories
  ↓
beforeEach hook
  ↓
requiresAuth: true → isAuthenticated() = true ✅
  ↓
requiresAdmin: true → hasRole('Administrator') = true ✅
  ↓
Load component: CategoriesManagement.vue
  ↓
Hiển thị: Trang quản lý categories
```

## Testing

### 1. Test Với User Thường (Không Có Quyền)

```javascript
// Login với user thường
await authStore.login('user@example.com', 'password')

// User data sẽ có roles: []
console.log(authStore.user?.roles) // []

// Kiểm tra role
console.log(authStore.hasRole('Administrator')) // false

// Thử truy cập management
router.push({ name: 'ManagementCategories' })
// Kết quả: Redirect đến AccessDenied
```

### 2. Test Với Admin

```javascript
// Login với admin
await authStore.login('admin@example.com', 'password')

// User data sẽ có roles: ['Administrator']
console.log(authStore.user?.roles) // ['Administrator']

// Kiểm tra role
console.log(authStore.hasRole('Administrator')) // true

// Truy cập management
router.push({ name: 'ManagementCategories' })
// Kết quả: Hiển thị trang quản lý ✅
```

### 3. Test Navigation Guard

```typescript
// Trong component test
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'

describe('Admin Routes', () => {
  it('should redirect non-admin users to AccessDenied', async () => {
    const authStore = useAuthStore()
    authStore.user = {
      id: '1',
      email: 'user@example.com',
      roles: [] // Không có role Administrator
    }

    await router.push('/management/categories')
    
    expect(router.currentRoute.value.name).toBe('AccessDenied')
  })

  it('should allow admin users to access management pages', async () => {
    const authStore = useAuthStore()
    authStore.user = {
      id: '1',
      email: 'admin@example.com',
      roles: ['Administrator'] // Có role Administrator
    }

    await router.push('/management/categories')
    
    expect(router.currentRoute.value.name).toBe('ManagementCategories')
  })
})
```

## Gán Quyền Admin Cho User

### Qua Database

```sql
-- Tạo user admin
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed)
VALUES (NEWID(), 'admin@example.com', 'admin@example.com', 1);

-- Gán role Administrator
-- (Cần có RoleId của role Administrator)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'admin@example.com' 
  AND r.Name = 'Administrator';
```

### Qua Backend API

```csharp
// UserController hoặc AdminController
[Authorize(Roles = Roles.Administrator)]
[HttpPost("users/{userId}/roles")]
public async Task<IActionResult> AssignRole(string userId, [FromBody] string roleName)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    var result = await _userManager.AddToRoleAsync(user, roleName);
    
    if (result.Succeeded)
        return Ok();
    
    return BadRequest(result.Errors);
}
```

## Console Logs

### Khi User Không Có Quyền

```
⚠️ Access denied: User does not have Administrator role
→ Redirect to: AccessDenied
```

### Khi User Có Quyền

```
✅ User has Administrator role
→ Allowing access to: ManagementCategories
```

## UI/UX Considerations

### Ẩn Menu Items Cho Non-Admin

File: `Web/ClientApp/src/components/Navigation.vue`

```vue
<template>
  <nav>
    <!-- Menu chỉ hiển thị cho Admin -->
    <div v-if="authStore.hasRole('Administrator')">
      <router-link :to="{ name: 'ManagementCategories' }">
        Categories Management
      </router-link>
      <router-link :to="{ name: 'ManagementVocabularies' }">
        Vocabularies Management
      </router-link>
      <router-link :to="{ name: 'ManagementExamples' }">
        Examples Management
      </router-link>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>
```

### Access Denied Page

File: `Web/ClientApp/src/modules/shared/views/AccessDenied.vue`

```vue
<template>
  <div class="access-denied">
    <h1>🚫 Truy Cập Bị Từ Chối</h1>
    <p>Bạn không có quyền truy cập trang này.</p>
    <p>Vui lòng liên hệ quản trị viên nếu bạn cần quyền truy cập.</p>
    
    <button @click="router.push({ name: 'Dashboard' })">
      Quay về Dashboard
    </button>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>
```

## Bảo Mật

### Backend Validation (Luôn Luôn Cần)

**Quan trọng**: Frontend authorization CHỈ là UX, backend PHẢI validate lại!

```csharp
// Backend API Controllers
[Authorize(Roles = Roles.Administrator)]
[HttpGet("categories")]
public async Task<IActionResult> GetCategories()
{
    // Chỉ admin mới gọi được API này
    var categories = await _mediator.Send(new GetCategoriesQuery());
    return Ok(categories);
}
```

### Double Check

```
Frontend Guard: Ngăn user click vào menu/URL
     +
Backend Authorization: Ngăn API calls
     =
Bảo mật đầy đủ ✅
```

## Routes Được Bảo Vệ

### Yêu Cầu Administrator Role

- ✅ `/management/categories` - Categories Management
- ✅ `/management/vocabularies` - Vocabularies Management  
- ✅ `/management/examples` - Examples Management

### Chỉ Yêu Cầu Authentication (Bất kỳ user nào đã đăng nhập)

- `/learning/categories` - Vocabulary Learning
- `/learning/words` - Word Learning
- `/learning/examples` - Example Learning
- `/profile` - User Profile
- `/dashboard` - Dashboard

### Public Routes (Không cần đăng nhập)

- `/login` - Login Page
- `/register` - Register Page
- `/access-denied` - Access Denied Page

## Troubleshooting

### User Không Thể Truy Cập Dù Là Admin

**Kiểm tra:**

1. User profile đã load đúng chưa?
```javascript
console.log(authStore.user)
// Phải có: { id: '...', email: '...', roles: ['Administrator'] }
```

2. hasRole() hoạt động đúng không?
```javascript
console.log(authStore.hasRole('Administrator'))
// Phải trả về: true
```

3. Role name trong database đúng không?
```sql
SELECT * FROM AspNetRoles WHERE Name = 'Administrator'
-- Phải khớp chính xác (case-sensitive)
```

### Guard Không Chạy

**Kiểm tra:**

1. Router đã import authStore chưa?
```typescript
import { useAuthStore } from '@/stores/auth'
```

2. loadUserProfile có được gọi không?
```typescript
console.log('Loading user profile...')
await authStore.loadUserProfile()
console.log('User:', authStore.user)
```

3. beforeEach có await không?
```typescript
router.beforeEach(async (to, from) => {
  // Phải có async/await
  await authStore.loadUserProfile()
})
```

## Mở Rộng Trong Tương Lai

### Thêm Roles Khác

```typescript
// Domain/Constants/Roles.cs
public static class Roles
{
    public const string Administrator = nameof(Administrator);
    public const string Moderator = nameof(Moderator);
    public const string PremiumUser = nameof(PremiumUser);
}

// Router meta
meta: { 
  requiresAuth: true,
  requiresRoles: ['Administrator', 'Moderator'] // Một trong hai
}

// Navigation guard
if (to.meta.requiresRoles) {
  const hasRequiredRole = to.meta.requiresRoles.some(
    role => authStore.hasRole(role)
  )
  
  if (!hasRequiredRole) {
    return { name: 'AccessDenied' }
  }
}
```

### Permission-Based (Thay vì Role-Based)

```typescript
// Kiểm tra permissions cụ thể
meta: {
  requiresPermissions: ['categories.edit', 'categories.delete']
}

// Guard
const hasPermission = (permission: string) => {
  return authStore.user?.permissions?.includes(permission) ?? false
}
```

## Tài Liệu Liên Quan

- `Web/ClientApp/src/router/index.ts` - Router configuration
- `Web/ClientApp/src/stores/auth.ts` - Authentication store
- `Web/ClientApp/src/types/router.d.ts` - TypeScript definitions
- `Domain/Constants/Roles.cs` - Backend role constants
- `Web/ClientApp/src/modules/shared/views/AccessDenied.vue` - Access denied page
