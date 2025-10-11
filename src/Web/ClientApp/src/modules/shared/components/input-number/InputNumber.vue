<script setup lang="ts">
import { formatNumeral, unformatNumeral, getSeparator } from './InputNumber.util'
import type { FormatNumeralOptions } from './InputNumber.type'
import { useEventListener } from '@vueuse/core'

const { options = {}, modelValue } = defineProps<{
  options?: Partial<FormatNumeralOptions>
  modelValue: number
}>()
const emits = defineEmits(['update:modelValue'])

const rawValue = ref<number>()

const inputRef = ref<HTMLInputElement | null>(null)

const defaultOptions = computed(() => {
  return {
    delimiter: getSeparator('group'),
    numeralDecimalMark: getSeparator('decimal'),
    numeralDecimalScale: 2,
    numeralIntegerScale: 16,
    ...options
  }
})
function formatValues(targetValue: string) {
  if (inputRef.value) {
    const formattedTargetValue = formatNumeral({ value: targetValue, ...defaultOptions.value })
    const safeTargetValue = getSafeValueString(unformatNumeral(formattedTargetValue ?? '', { ...defaultOptions.value }))

    inputRef.value.value = formatNumeral({
      value: safeTargetValue.replace('.', defaultOptions.value.numeralDecimalMark),
      ...defaultOptions.value
    })
    const formattedTargetValueAsNumber = Number(safeTargetValue)
    if (!isNaN(formattedTargetValueAsNumber)) {
      rawValue.value = formattedTargetValueAsNumber
    } else {
      rawValue.value = undefined
    }
  }
}
function truncateRedundantDecimalMark(targetValue: string) {
  if (
    inputRef.value &&
    targetValue &&
    targetValue[targetValue.length - 1] === defaultOptions.value.numeralDecimalMark
  ) {
    inputRef.value.value = targetValue.substring(0, targetValue.length - 1)
  }
}
function getSafeValueString(value: string) {
  const numberValue = Number(value)
  if (numberValue > Number.MAX_SAFE_INTEGER) {
    return Number.MAX_SAFE_INTEGER.toString()
  } else if (numberValue < Number.MIN_SAFE_INTEGER) {
    return Number.MIN_SAFE_INTEGER.toString()
  }
  return value
}
useEventListener(
  inputRef,
  'input',
  (
    e: Event & {
      target: HTMLInputElement
    }
  ) => {
    formatValues(e.target.value)
  }
)

useEventListener(
  inputRef,
  'blur',
  (
    e: Event & {
      target: HTMLInputElement
    }
  ) => {
    if (e.target.value) {
      truncateRedundantDecimalMark(e.target.value)
    }
  }
)
onMounted(() => {
  if (!inputRef.value) {
    return
  }
  inputRef.value.value = formatNumeral({
    value: rawValue.value?.toString().replace('.', defaultOptions.value.numeralDecimalMark),
    ...defaultOptions.value
  })
})
watch(
  () => modelValue,
  (value) => {
    if (value !== rawValue.value) {
      rawValue.value = value
      if (inputRef.value) {
        inputRef.value.value = formatNumeral({
          value: rawValue.value?.toString().replace('.', defaultOptions.value.numeralDecimalMark),
          ...defaultOptions.value
        })
      }
    }
  },
  {
    immediate: true
  }
)
watch(rawValue, (raw) => {
  if (raw !== modelValue) {
    emits('update:modelValue', raw)
  }
})
</script>
<template>
  <input type="text" ref="inputRef" />
</template>
