import { shallowMount } from '@vue/test-utils'
import { test, expect } from 'vitest'
import Button from './Button.vue'

test('Button renders correctly', () => {
  const wrapper = shallowMount(Button)
  expect(wrapper.html()).toContain('button')
})

test('Button binds attributes correctly', () => {
  const wrapper = shallowMount(Button, {
    attrs: {
      id: 'id',
      disabled: true
    }
  })
  expect(wrapper.attributes('id')).toBe('id')
  expect(wrapper.attributes('disabled')).toBe('true')
})
