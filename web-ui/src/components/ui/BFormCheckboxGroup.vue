<template>
  <div class="space-y-2">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'BFormCheckboxGroup',
  props: {
    modelValue: Array,
    id: String,
    stacked: Boolean
  }
};
</script>
