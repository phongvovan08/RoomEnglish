import { test, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import { usePromiseWrapper } from './usePromiseWrapper'

test('usePromiseWrapper', () => {
  const mockFn = vi.fn()
  const TestComponent = defineComponent({
    setup() {
      const { isLoading, execute } = usePromiseWrapper({
        key: 'test',
        promiseFn: async () => mockFn(),
      })
      return { execute, isLoading }
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
  wrapper.vm.execute()
  expect(mockFn).toHaveBeenCalledTimes(1)
})
