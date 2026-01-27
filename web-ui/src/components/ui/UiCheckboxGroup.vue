<template>
  <div class="space-y-2">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed, provide } from 'vue'

interface Props {
  modelValue?: (string | number | boolean)[]
  id?: string
  stacked?: boolean
  switches?: boolean
  size?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
  stacked: false,
  switches: false
})

const emit = defineEmits<{
  'update:modelValue': [value: (string | number | boolean)[]]
}>()

function updateValue(checked: boolean, value: string | number | boolean) {
  const newValue = [...(props.modelValue || [])]
  const index = newValue.indexOf(value)

  if (checked && index === -1) {
    newValue.push(value)
  } else if (!checked && index !== -1) {
    newValue.splice(index, 1)
  }

  emit('update:modelValue', newValue)
}

provide('checkboxGroup', computed(() => ({
  value: props.modelValue || [],
  updateValue
})))
</script>
