<template>
  <div class="space-y-2">
    <slot></slot>
  </div>
</template>

<script>
import { computed } from 'vue'

export default {
  name: 'BFormCheckboxGroup',
  props: {
    modelValue: Array,
    id: String,
    stacked: Boolean,
    switches: Boolean,
    size: String
  },
  provide() {
    return {
      checkboxGroup: computed(() => ({
        value: this.modelValue || [],
        updateValue: this.updateValue
      }))
    }
  },
  methods: {
    updateValue(checked, value) {
      const newValue = [...(this.modelValue || [])]
      const index = newValue.indexOf(value)
      
      if (checked && index === -1) {
        newValue.push(value)
      } else if (!checked && index !== -1) {
        newValue.splice(index, 1)
      }
      
      this.$emit('update:modelValue', newValue)
    }
  },
  emits: ['update:modelValue']
};
</script>
