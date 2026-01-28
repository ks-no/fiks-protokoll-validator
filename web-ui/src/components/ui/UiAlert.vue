<template>
  <div
    v-if="modelValue"
    :class="alertClasses"
    role="alert"
  >
    <slot />
    <button
      v-if="dismissible"
      class="ml-auto text-xl leading-none opacity-70 hover:opacity-100"
      aria-label="Close"
      @click="emit('update:modelValue', false)"
    >
      &times;
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type AlertVariant = 'success' | 'danger' | 'warning' | 'info'

interface Props {
  modelValue?: boolean
  variant?: AlertVariant
  dismissible?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: true,
  variant: 'info',
  dismissible: false
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const alertClasses = computed(() => {
  const baseClasses = 'flex items-center px-4 py-3 rounded border mb-4'
  const variants: Record<AlertVariant, string> = {
    success: 'bg-green-100 border-green-400 text-green-700',
    danger: 'bg-red-100 border-red-400 text-red-700',
    warning: 'bg-yellow-100 border-yellow-400 text-yellow-700',
    info: 'bg-blue-100 border-blue-400 text-blue-700'
  }
  return `${baseClasses} ${variants[props.variant]}`
})
</script>
