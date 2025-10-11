import { describe, test, expect } from 'vitest'
import SvgIcon from './SvgIcon.vue'
import { mount } from '@vue/test-utils'

describe('SVG Icon', () => {
  const wrapper = mount(SvgIcon, {
    props: {
      name: 'logo',
    },
  })
  test('Should not render render svg icon when pass invalid name', async () => {
    wrapper.setProps({ name: 'invalidName' })
    await wrapper.vm.$nextTick()
    const svg = wrapper.find('svg')
    expect(svg.exists()).toBeFalsy()
  })
})
