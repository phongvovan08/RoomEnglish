import { test, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import { useQuery } from './useQuery'

test('useQuery', () => {
  const mockFn = vi.fn()
  const TestComponent = defineComponent({
    setup() {
      const { isLoading, execute } = useQuery({
        key: 'test',
        promiseFn: async () => mockFn(),
        secondsToLive: 0
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
