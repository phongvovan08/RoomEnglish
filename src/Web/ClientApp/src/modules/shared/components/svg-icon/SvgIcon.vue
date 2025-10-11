<template>
  <span
    v-if="icon"
    class="svg-icon"
    :class="{
      'svg-icon--fill': !filled,
      'svg-icon--stroke': hasStroke && !filled,
      'svg-icon--parent-size': parentSize,
    }"
    v-html="icon"
  >
  </span>
</template>

<script setup lang="ts">
import { ref, watchEffect } from 'vue'
import type { SvgIconProps } from './SvgIcon.type'

const { name, filled = false, parentSize = false } = defineProps<SvgIconProps>()

const icon = ref<string | Record<string, any>>('')
const hasStroke = ref(false)
async function getIcon() {
  if (name === '') {
    icon.value = ''
    return
  }
  try {
    const iconsImport = import.meta.glob('@/assets/icons/**/**.svg', {
      query: '?raw',
      eager: false,
      import: 'default',
    })
    const rawIcon = (await iconsImport[`/src/assets/icons/${name}.svg`]()) as string
    if (rawIcon.includes('stroke')) {
      hasStroke.value = true
    }
    icon.value = rawIcon
  } catch {
    icon.value = ''
    console.error(`[svg-icons] Icon '${name}' doesn't exist in 'assets/icons'`)
  }
}

watchEffect(getIcon)
</script>

<style>
.svg-icon svg {
  width: 1em;
  height: 1em;
  margin-bottom: 0.125em;
  vertical-align: middle;
}

.svg-icon.svg-icon--fill,
.svg-icon.svg-icon--fill * {
  fill: currentColor !important;
}

.svg-icon.svg-icon--stroke,
.svg-icon.svg-icon--stroke * {
  stroke: currentColor !important;
}

.svg-icon.svg-icon--parent-size svg {
  width: 100%;
  height: 100%;
}
</style>
