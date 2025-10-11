import type { App } from 'vue'
/**
 * UI Dependencies
 */
import { createPinia } from 'pinia'
import i18n, { loadLanguageAsync } from './i18n'
import router from '@/router'
import { createHead } from '@unhead/vue'
import { Icon } from "@iconify/vue";
import { addPrimeVueDependencies } from './primevue'

const head = createHead()

export function addUIDependencies(app: App): void {
  app.use(createPinia())
  app.use(i18n)
  app.use(router)
  app.use(head)
  app.component('Icon', Icon)
  addPrimeVueDependencies(app)
  loadLanguageAsync('en')
}
