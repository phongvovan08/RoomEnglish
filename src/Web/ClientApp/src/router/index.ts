import { msalService } from "@/plugins/services/services";
import { createRouter, createWebHistory } from "vue-router";
import { Routes } from "./constants";
import postsRoutes from "../modules/posts/router";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      alias: "/home/index",
      redirect: { name: Routes.Posts.name },
    },
    {
      ...Routes.AccessDenied,
      component: () => import("../modules/shared/views/AccessDenied.vue"),
      meta: {
        public: true,
      },
    },
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
