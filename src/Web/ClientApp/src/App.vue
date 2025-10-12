<script lang="ts" setup>
import DefaultLayout from "./modules/shared/layouts/DefaultLayout.vue";
import { useHead } from "@unhead/vue";
import Toast from './modules/shared/components/toast';

const { t } = useI18n();

useHead({
  title: () => t("app.title"),
});
</script>

<template>
  <DefaultLayout>
    <RouterView />
    <Toast />
  </DefaultLayout>
</template>
