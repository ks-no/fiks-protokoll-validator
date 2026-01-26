<template>
  <div
    v-if="show"
    :class="alertClasses"
    role="alert"
  >
    <slot></slot>
    <button 
      v-if="dismissible"
      @click="$emit('input', false)"
      class="ml-auto text-xl leading-none opacity-70 hover:opacity-100"
      aria-label="Close"
    >
      ×
    </button>
  </div>
</template>

<script>
export default {
  name: 'BAlert',
  props: {
    show: {
      type: Boolean,
      default: true
    },
    variant: {
      type: String,
      default: 'info'
    },
    dismissible: Boolean
  },
  computed: {
    alertClasses() {
      const baseClasses = 'flex items-center px-4 py-3 rounded border mb-4';
      const variants = {
        success: 'bg-green-100 border-green-400 text-green-700',
        danger: 'bg-red-100 border-red-400 text-red-700',
        warning: 'bg-yellow-100 border-yellow-400 text-yellow-700',
        info: 'bg-blue-100 border-blue-400 text-blue-700'
      };
      return `${baseClasses} ${variants[this.variant] || variants.info}`;
    }
  }
};
</script>
