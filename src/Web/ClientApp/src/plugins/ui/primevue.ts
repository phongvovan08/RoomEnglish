import type { App } from 'vue'
import Tooltip from 'primevue/tooltip'
import ToastService from 'primevue/toastservice'
import BadgeDirective from 'primevue/badgedirective'
import ConfirmationService from 'primevue/confirmationservice'
import Aura from '@primeuix/themes/aura'
import PrimeVue from 'primevue/config'
import { definePreset } from '@primeuix/themes'

const MyPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{blue.50}',
      100: '{blue.100}',
      200: '{blue.200}',
      300: '{blue.300}',
      400: '{blue.400}',
      500: '{blue.500}',
      600: '{blue.600}',
      700: '{blue.700}',
      800: '{blue.800}',
      900: '{blue.900}',
      950: '{blue.950}'
    }
  }
})

export function addPrimeVueDependencies(app: App) {
  app.use(PrimeVue, {
    theme: {
      preset: MyPreset,
      options: {
        darkModeSelector: false,
        cssLayer: {
          name: 'primevue',
          order: 'theme, base, primevue'
        }
      }
    }
  })
  app.use(ToastService)
  app.directive('tooltip', Tooltip)
  app.directive('badge', BadgeDirective)
  app.use(ConfirmationService)
}
