export const Routes = {
  // System routes
  AccessDenied: {
    name: "AccessDenied",
    path: "/access-denied",
  },
  Home: {
    name: "Home",
    path: "/",
  },
  
  // Authentication routes
  Login: {
    name: "Login",
    path: "/login",
  },
  
  // Dashboard routes
  Dashboard: {
    name: "Dashboard",
    path: "/dashboard",
  },
  
  // TodoLists management (mapping to backend /api/TodoLists)
  TodoLists: {
    name: "TodoLists",
    path: "/todo-lists",
    children: {
      List: {
        name: "TodoListsList",
        path: "/todo-lists",
      },
      Create: {
        name: "CreateTodoList",
        path: "/todo-lists/create",
      },
      Edit: {
        name: "EditTodoList", 
        path: "/todo-lists/:id/edit",
      },
      View: {
        name: "ViewTodoList",
        path: "/todo-lists/:id",
      },
    },
  },
  
  // TodoItems management (mapping to backend /api/TodoItems)
  TodoItems: {
    name: "TodoItems",
    path: "/todo-items",
    children: {
      List: {
        name: "TodoItemsList",
        path: "/todo-items",
      },
      Create: {
        name: "CreateTodoItem",
        path: "/todo-items/create",
      },
      Edit: {
        name: "EditTodoItem",
        path: "/todo-items/:id/edit",
      },
      View: {
        name: "ViewTodoItem", 
        path: "/todo-items/:id",
      },
    },
  },
  
  // Weather Forecast (mapping to backend /api/WeatherForecasts)
  WeatherForecasts: {
    name: "WeatherForecasts",
    path: "/weather-forecasts",
  },
  
  // Users management (mapping to backend /api/Users)
  Users: {
    name: "Users",
    path: "/users",
    children: {
      Profile: {
        name: "UserProfile",
        path: "/users/profile",
      },
      Settings: {
        name: "UserSettings",
        path: "/users/settings",
      },
    },
  },
  
  // Legacy Posts (keeping existing)
  Posts: {
    name: "Posts",
    path: "/posts",
  },
};

