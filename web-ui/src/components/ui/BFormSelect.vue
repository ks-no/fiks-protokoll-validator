<template>
  <select
    :value="modelValue"
    @change="handleChange"
    class="block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
  >
    <option v-for="option in options" :key="option.value" :value="option.value" :disabled="option.disabled">
      {{ option.text }}
    </option>
  </select>
</template>

<script setup lang="ts">
interface SelectOption {
  value: string
  text: string
  disabled?: boolean
}

interface Props {
  modelValue?: string | number
  options: SelectOption[]
}

defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
  change: [value: string]
}>()

function handleChange(event: Event) {
  const target = event.target as HTMLSelectElement
  emit('update:modelValue', target.value)
  emit('change', target.value)
}
</script>
