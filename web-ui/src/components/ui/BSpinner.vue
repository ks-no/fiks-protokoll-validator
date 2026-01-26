<template>
  <div class="animate-spin rounded-full border-b-2" :class="spinnerClasses" :style="spinnerStyle">
    <span v-if="label" class="sr-only">{{ label }}</span>
  </div>
</template>

<script>
export default {
  name: 'BSpinner',
  props: {
    variant: {
      type: String,
      default: 'primary'
    },
    label: String,
    small: Boolean
  },
  computed: {
    spinnerClasses() {
      const variants = {
        primary: 'border-blue-600',
        secondary: 'border-gray-600',
        success: 'border-green-600',
        danger: 'border-red-600',
        warning: 'border-yellow-600'
      };
      return variants[this.variant] || variants.primary;
    },
    spinnerStyle() {
      const size = this.small ? '1rem' : '1.5rem';
      return {
        width: size,
        height: size,
        verticalAlign: '8px',
        marginLeft: '10px'
      };
    }
  }
};
</script>
