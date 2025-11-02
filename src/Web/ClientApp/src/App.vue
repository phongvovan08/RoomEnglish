<script lang="ts" setup>
import DefaultLayout from "./modules/shared/layouts/DefaultLayout.vue";
import { useHead } from "@unhead/vue";
import NotificationToast from '@/components/ui/NotificationToast.vue'
import VersionBadge from '@/components/VersionBadge.vue'

const { t } = useI18n();

useHead({
  title: () => t("app.title"),
});
</script>

<template>
  <DefaultLayout>
    <RouterView />
    <NotificationToast />
    <VersionBadge />
  </DefaultLayout>
</template>
