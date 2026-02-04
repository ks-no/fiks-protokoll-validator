<template>
  <span
    class="inline-block animate-spin rounded-full border-2 border-transparent"
    :class="spinnerClasses"
    :style="spinnerStyle"
  >
    <span
      v-if="label"
      class="sr-only"
    >{{ label }}</span>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type SpinnerVariant = 'primary' | 'secondary' | 'success' | 'danger' | 'warning'

interface Props {
  variant?: SpinnerVariant
  label?: string
  small?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary'
})

const spinnerClasses = computed(() => {
  const variants: Record<SpinnerVariant, string> = {
    primary: 'border-t-blue-600',
    secondary: 'border-t-gray-600',
    success: 'border-t-green-600',
    danger: 'border-t-red-600',
    warning: 'border-t-yellow-600'
  }
  return variants[props.variant]
})

const spinnerStyle = computed(() => {
  const size = props.small ? '1rem' : '1.5rem'
  return {
    width: size,
    height: size
  }
})
</script>
