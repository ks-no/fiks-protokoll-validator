<template>
  <div class="flex flex-wrap" :class="rowClasses">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'BRow',
  props: {
    noGutters: Boolean
  },
  computed: {
    rowClasses() {
      return this.noGutters ? '' : '-mx-4';
    }
  }
};
</script>
