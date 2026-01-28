<template>
  <label class="inline-flex items-center cursor-pointer">
    <input
      :id="id"
      type="checkbox"
      :checked="isChecked"
      :value="value"
      :class="[
        switchStyle ? 'toggle-switch' : 'w-4 h-4',
        'text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500'
      ]"
      @change="handleChange"
    >
    <span class="ml-2"><slot /></span>
  </label>
</template>

<script setup lang="ts">
import { computed, inject, type ComputedRef } from 'vue'

interface CheckboxGroupContext {
  value: (string | number | boolean)[]
  updateValue: (checked: boolean, value: string | number | boolean) => void
}

interface Props {
  modelValue?: boolean
  value?: string | number | boolean
  id?: string
  switch?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  value: undefined,
  id: undefined,
  switch: false
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  change: [checked: boolean]
}>()

const checkboxGroup = inject<ComputedRef<CheckboxGroupContext> | null>('checkboxGroup', null)

const switchStyle = computed(() => props.switch)

const isChecked = computed(() => {
  if (checkboxGroup?.value) {
    const group = checkboxGroup.value
    return group.value?.includes(props.value as string | number | boolean) ?? false
  }
  return props.modelValue
})

function handleChange(event: Event) {
  const target = event.target as HTMLInputElement
  const checked = target.checked

  if (checkboxGroup?.value) {
    const group = checkboxGroup.value
    group.updateValue(checked, props.value as string | number | boolean)
  } else {
    emit('update:modelValue', checked)
  }

  emit('change', checked)
}
</script>

<style scoped>
.toggle-switch {
  width: 3rem;
  height: 1.5rem;
  appearance: none;
  background-color: #cbd5e0;
  border-radius: 9999px;
  position: relative;
  cursor: pointer;
  transition: background-color 0.2s;
}

.toggle-switch:checked {
  background-color: #3b82f6;
}

.toggle-switch::before {
  content: '';
  position: absolute;
  width: 1.25rem;
  height: 1.25rem;
  border-radius: 50%;
  background-color: white;
  top: 0.125rem;
  left: 0.125rem;
  transition: transform 0.2s;
}

.toggle-switch:checked::before {
  transform: translateX(1.5rem);
}
</style>
