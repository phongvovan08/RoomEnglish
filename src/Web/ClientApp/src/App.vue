<script lang="ts" setup>
import DefaultLayout from "./modules/shared/layouts/DefaultLayout.vue";
import { useHead } from "@unhead/vue";
import { loadLanguageAsync } from "./plugins/ui/i18n";
import Toast from './modules/shared/components/toast';
import InputNumber from "./modules/shared/components/input-number";

const { t, locale } = useI18n();

const value = ref(111111.23)

function updateValue() {
  value.value = 123123123
}
useHead({
  title: () => t("app.title"),
});
</script>

<template>
  <DefaultLayout>
    <div class="flex items-center justify-between">
      <h1 class="text-5xl font-bold mb-8">{{ $t("app.title") }}</h1>
      <button
        @click="
          locale === 'en' ? loadLanguageAsync('fr') : loadLanguageAsync('en')
        "
        class="border border-gray-300 rounded-md px-4 cursor-pointer py-2 flex items-center gap-2"
      >
        Change language <Icon icon="fluent-mdl2:locale-language" />
      </button>
      <button @click="updateValue">updateValue</button>
      <InputNumber v-model="value" class="border border-solid border-gray-400 p-2 rounded" />
    </div>
    <RouterView />
    <Toast />
  </DefaultLayout>
</template>
