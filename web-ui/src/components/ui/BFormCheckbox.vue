<template>
  <label class="inline-flex items-center cursor-pointer">
    <input
      type="checkbox"
      :checked="isChecked"
      :value="value"
      :id="id"
      @change="handleChange"
      :class="[
        switchStyle ? 'toggle-switch' : 'w-4 h-4',
        'text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500'
      ]"
    />
    <span class="ml-2"><slot></slot></span>
  </label>
</template>

<script>
export default {
  name: 'BFormCheckbox',
  inject: {
    checkboxGroup: { default: null }
  },
  props: {
    modelValue: Boolean,
    value: [String, Boolean, Number],
    id: String,
    switch: Boolean
  },
  computed: {
    switchStyle() {
      return this.switch
    },
    isChecked() {
      if (this.checkboxGroup) {
        // Part of a group - unwrap computed if needed
        const group = typeof this.checkboxGroup === 'function' ? this.checkboxGroup() : this.checkboxGroup
        return group.value && group.value.includes(this.value)
      }
      // Standalone checkbox
      return this.modelValue
    }
  },
  methods: {
    handleChange(event) {
      const checked = event.target.checked
      
      if (this.checkboxGroup) {
        // Part of a group - unwrap computed if needed
        const group = typeof this.checkboxGroup === 'function' ? this.checkboxGroup() : this.checkboxGroup
        group.updateValue(checked, this.value)
      } else {
        // Standalone checkbox
        this.$emit('update:modelValue', checked)
      }
      
      this.$emit('change', checked)
    }
  },
  emits: ['update:modelValue', 'change']
};
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
