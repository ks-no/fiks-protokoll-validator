<template>
  <select
    :value="modelValue"
    @change="$emit('update:modelValue', $event.target.value); $emit('change', $event.target.value)"
    class="block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
  >
    <option v-for="option in options" :key="option.value" :value="option.value">
      {{ option.text }}
    </option>
  </select>
</template>

<script>
export default {
  name: 'BFormSelect',
  props: {
    modelValue: [String, Number],
    options: Array
  },
  emits: ['update:modelValue', 'change']
};
</script>
