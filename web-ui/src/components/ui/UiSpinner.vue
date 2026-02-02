<template>
  <div
    class="animate-spin rounded-full border-b-2"
    :class="spinnerClasses"
    :style="spinnerStyle"
  >
    <span
      v-if="label"
      class="sr-only"
    >{{ label }}</span>
  </div>
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
    primary: 'border-blue-600',
    secondary: 'border-gray-600',
    success: 'border-green-600',
    danger: 'border-red-600',
    warning: 'border-yellow-600'
  }
  return variants[props.variant]
})

const spinnerStyle = computed(() => {
  const size = (props.small ?? false) ? '1rem' : '1.5rem'
  return {
    width: size,
    height: size,
    verticalAlign: '8px',
    marginLeft: '10px'
  }
})
</script>
