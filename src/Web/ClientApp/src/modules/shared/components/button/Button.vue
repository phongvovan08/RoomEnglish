<script lang="ts" setup>
import Button from 'primevue/button'
import { useAttrs } from 'vue'

const attrs = useAttrs()
</script>

<template>
  <Button v-bind="attrs"><slot /></Button>
</template>
