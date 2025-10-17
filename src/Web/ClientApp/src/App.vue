<script lang="ts" setup>
import DefaultLayout from "./modules/shared/layouts/DefaultLayout.vue";
import { useHead } from "@unhead/vue";
import NotificationToast from '@/components/ui/NotificationToast.vue'
import FloatingVocabularyButton from '@/components/FloatingVocabularyButton.vue'

const { t } = useI18n();

useHead({
  title: () => t("app.title"),
});
</script>

<template>
  <DefaultLayout>
    <RouterView />
    <NotificationToast />
    <FloatingVocabularyButton />
  </DefaultLayout>
</template>
