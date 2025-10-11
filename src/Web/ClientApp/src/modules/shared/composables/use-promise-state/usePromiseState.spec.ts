import { test, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import { usePromiseState } from './usePromiseState'

test('usePromiseWrapper', () => {
  const TestComponent = defineComponent({
    setup() {
      const { isLoading } = usePromiseState('test')
      return { isLoading }
    },
    template: '<div></div>',
  })
  const pinia = createPinia()
  setActivePinia(pinia)
  const wrapper = mount(TestComponent, {
    global: {
      plugins: [pinia],
    },
  })
  expect(wrapper.exists()).toBeTruthy()
  expect(wrapper.vm.isLoading).toBeFalsy()

})
