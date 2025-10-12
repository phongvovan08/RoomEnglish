import { Routes } from '@/router/constants'

export default [
  {
    path: Routes.Dashboard.path,
    name: Routes.Dashboard.name,
    component: () => import('../views/DashboardView.vue'),
    meta: {
      requiresAuth: true,
    },
  },
]