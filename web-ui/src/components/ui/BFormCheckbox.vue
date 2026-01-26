<template>
  <label class="inline-flex items-center cursor-pointer">
    <input
      type="checkbox"
      :checked="modelValue"
      :value="value"
      :id="id"
      :switch="switch"
      @change="$emit('update:modelValue', $event.target.checked); $emit('change', $event.target.checked)"
      class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
    />
    <span class="ml-2"><slot></slot></span>
  </label>
</template>

<script>
export default {
  name: 'BFormCheckbox',
  props: {
    modelValue: Boolean,
    value: [String, Boolean],
    id: String,
    switch: Boolean
  },
  emits: ['update:modelValue', 'change']
};
</script>
