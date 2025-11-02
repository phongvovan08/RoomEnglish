# Frontend-Backend Route Mapping

## Overview
This document describes how the Vue 3 frontend routes map to the ASP.NET Core backend API endpoints.

## Route Structure

### Backend API Endpoints
- Base URL: `https://localhost:5001/api`
- All endpoints follow REST conventions

### Frontend Routes Mapping

#### 1. Authentication
**Backend:** `/api/Authentication`
**Frontend:** Used for authentication only, no direct routes
- `POST /api/Authentication/GetToken` - Get token with credentials
- `POST /api/Authentication/GetDefaultToken` - Get token with default admin account

#### 2. Users Management
**Backend:** `/api/Users`
**Frontend:** `/users`
- `POST /api/Users/login` → Login functionality
- `POST /api/Users/register` → Registration functionality
- `GET /users/profile` → User profile page
- `GET /users/settings` → User settings page

#### 3. Todo Lists
**Backend:** `/api/TodoLists`
**Frontend:** `/todo-lists`
- `GET /api/TodoLists` → `GET /todo-lists` (List all todo lists)
- `POST /api/TodoLists` → `POST /todo-lists/create` (Create new todo list form)
- `PUT /api/TodoLists/{id}` → `PUT /todo-lists/{id}/edit` (Edit todo list form)
- `DELETE /api/TodoLists/{id}` → `DELETE` action from list view
- `GET /todo-lists/{id}` → View todo list details

#### 4. Todo Items
**Backend:** `/api/TodoItems`
**Frontend:** `/todo-items`
- `GET /api/TodoItems` → `GET /todo-items` (List todo items with pagination)
- `POST /api/TodoItems` → `POST /todo-items/create` (Create new todo item form)
- `PUT /api/TodoItems/{id}` → `PUT /todo-items/{id}/edit` (Edit todo item form)
- `PUT /api/TodoItems/UpdateItemDetails?Id={id}` → Update item details
- `DELETE /api/TodoItems/{id}` → `DELETE` action from list view

#### 5. Weather Forecasts
**Backend:** `/api/WeatherForecasts`
**Frontend:** `/weather-forecasts`
- `GET /api/WeatherForecasts` → `GET /weather-forecasts` (Display weather data)

#### 6. Dashboard
**Frontend:** `/dashboard`
- Aggregates data from multiple backend endpoints
- Calls TodoLists, TodoItems, WeatherForecasts APIs to show statistics

## Menu Structure

### Responsive Navigation Menu
The menu is built with Vue 3 and Tailwind CSS, featuring:
- Desktop horizontal navigation with dropdowns
- Mobile hamburger menu with collapsible sections
- Active route highlighting
- Multi-language support (English/French)

### Menu Items Configuration
```typescript
const menuItems = [
  {
    name: 'dashboard',
    label: 'menu.dashboard',
    path: '/dashboard',
    icon: 'mdi:view-dashboard',
  },
  {
    name: 'todoLists',
    label: 'menu.todoLists', 
    path: '/todo-lists',
    icon: 'mdi:format-list-bulleted-square',
    hasChildren: true,
    children: [
      { name: 'todoListsList', label: 'menu.viewAll', path: '/todo-lists' },
      { name: 'createTodoList', label: 'menu.create', path: '/todo-lists/create' },
    ],
  },
  // ... more menu items
]
```

## API Service Layer

### Service Classes
Each backend controller has a corresponding frontend service:

```typescript
// Maps to /api/TodoLists controller
export class TodoListsService {
  static async getAll() { /* GET /api/TodoLists */ }
  static async create(title: string) { /* POST /api/TodoLists */ }
  static async update(id: number, title: string) { /* PUT /api/TodoLists/{id} */ }
  static async delete(id: number) { /* DELETE /api/TodoLists/{id} */ }
}
```

### Authentication Integration
- Uses bearer token authentication
- Automatically includes tokens in API requests
- Handles token refresh and logout

## File Structure
```
src/
├── modules/
│   ├── dashboard/           # Dashboard aggregation views
│   ├── todo-lists/          # TodoLists CRUD operations  
│   ├── todo-items/          # TodoItems CRUD operations
│   ├── weather/             # Weather forecast display
│   ├── users/               # User management
│   └── shared/              # Shared components (menu, layout)
├── router/
│   ├── constants.ts         # Route definitions
│   └── index.ts             # Router configuration
├── services/
│   └── api.ts               # Backend API integration
└── locales/                 # i18n translations
```

## Features
- **Responsive Design**: Mobile-first approach with Tailwind CSS
- **Internationalization**: English and French support
- **Type Safety**: Full TypeScript integration
- **Authentication**: JWT token-based auth with ASP.NET Core Identity
- **Real-time Updates**: Components automatically refresh data
- **Error Handling**: Graceful error handling and user feedback
- **Loading States**: Visual feedback during API calls

## Development
1. Backend API must be running on `https://localhost:5001`
2. Frontend development server: `npm run dev`
3. API endpoints are automatically mapped through the service layer
4. Authentication tokens are managed automatically