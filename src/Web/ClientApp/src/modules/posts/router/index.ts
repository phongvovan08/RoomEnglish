import type { RouteRecordRaw } from 'vue-router'
import { Routes } from '@/router/constants'

const routes: RouteRecordRaw[] = [
  {
    ...Routes.Posts,
    component: () => import('../views/Posts.vue'),
    
  },  
]

export default routes
