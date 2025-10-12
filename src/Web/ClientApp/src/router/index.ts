import { msalService } from "@/plugins/services/services";
import { createRouter, createWebHistory } from "vue-router";
import { Routes } from "./constants";
import postsRoutes from "../modules/posts/router";
import dashboardRoutes from "../modules/dashboard/router";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      alias: "/home/index",
      redirect: { name: Routes.Dashboard.name },
    },
    {
      ...Routes.AccessDenied,
      component: () => import("../modules/shared/views/AccessDenied.vue"),
      meta: {
        public: true,
      },
    },
    // Dashboard routes
    ...dashboardRoutes,
    
    // TodoLists routes
    {
      path: Routes.TodoLists.path,
      name: Routes.TodoLists.name,
      component: () => import("../modules/todo-lists/views/TodoListsView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoLists.children.Create.path,
      name: Routes.TodoLists.children.Create.name,
      component: () => import("../modules/todo-lists/views/CreateTodoListView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoLists.children.Edit.path,
      name: Routes.TodoLists.children.Edit.name,
      component: () => import("../modules/todo-lists/views/EditTodoListView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoLists.children.View.path,
      name: Routes.TodoLists.children.View.name,
      component: () => import("../modules/todo-lists/views/ViewTodoListView.vue"),
      meta: { requiresAuth: true },
    },
    
    // TodoItems routes
    {
      path: Routes.TodoItems.path,
      name: Routes.TodoItems.name,
      component: () => import("../modules/todo-items/views/TodoItemsView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoItems.children.Create.path,
      name: Routes.TodoItems.children.Create.name,
      component: () => import("../modules/todo-items/views/CreateTodoItemView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoItems.children.Edit.path,
      name: Routes.TodoItems.children.Edit.name,
      component: () => import("../modules/todo-items/views/EditTodoItemView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.TodoItems.children.View.path,
      name: Routes.TodoItems.children.View.name,
      component: () => import("../modules/todo-items/views/ViewTodoItemView.vue"),
      meta: { requiresAuth: true },
    },
    
    // Weather Forecasts route
    {
      path: Routes.WeatherForecasts.path,
      name: Routes.WeatherForecasts.name,
      component: () => import("../modules/weather/views/WeatherForecastsView.vue"),
      meta: { requiresAuth: true },
    },
    
    // User routes
    {
      path: Routes.Users.children.Profile.path,
      name: Routes.Users.children.Profile.name,
      component: () => import("../modules/users/views/UserProfileView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: Routes.Users.children.Settings.path,
      name: Routes.Users.children.Settings.name,
      component: () => import("../modules/users/views/UserSettingsView.vue"),
      meta: { requiresAuth: true },
    },
    
    // Legacy posts routes
    ...postsRoutes,
  ],
});

router.beforeEach(async (to) => {
  if (import.meta.env.VITE_USE_MSAL_CLIENT) {
    if (!to.meta.public) {
      try {
        await msalService.checkIsAuthenticated(to.fullPath);
      } catch (error) {
        console.error(error);
        return { name: "AccessDenied" };
      }
    }
  }
});

export default router;
