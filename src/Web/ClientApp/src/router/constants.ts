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
  Auth: {
    name: "Auth",
    path: "/auth",
    children: {
      Login: {
        name: "Login",
        path: "/auth/login",
      },
      Register: {
        name: "Register",
        path: "/auth/register",
      },
      ForgotPassword: {
        name: "ForgotPassword",
        path: "/auth/forgot-password",
      },
    },
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
      ByList: {
        name: "TodoItemsByList",
        path: "/todo-lists/:listId/items",
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
  
  // Vocabulary Learning System (mapping to backend /api/Vocabulary)
  Vocabulary: {
    name: "Vocabulary",
    path: "/vocabulary",
    children: {
      Learning: {
        name: "VocabularyLearning",
        path: "/vocabulary/learning",
      },
      Categories: {
        name: "VocabularyCategories", 
        path: "/vocabulary/categories",
      },
      Words: {
        name: "VocabularyWords",
        path: "/vocabulary/words",
      },
      Practice: {
        name: "VocabularyPractice",
        path: "/vocabulary/practice",
      },
      Dictation: {
        name: "VocabularyDictation",
        path: "/vocabulary/dictation",
      },
      Progress: {
        name: "VocabularyProgress",
        path: "/vocabulary/progress",
      },
    },
  },
  
  // Legacy Posts (keeping existing)
  Posts: {
    name: "Posts",
    path: "/posts",
  },
};

