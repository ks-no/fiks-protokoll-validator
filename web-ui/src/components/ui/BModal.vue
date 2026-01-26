<template>
  <div v-if="modelValue" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <div class="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4">
      <div class="p-6">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'BModal',
  props: {
    modelValue: Boolean
  }
};
</script>
