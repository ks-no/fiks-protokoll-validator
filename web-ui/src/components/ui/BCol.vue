<template>
  <div :class="colClasses">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'BCol',
  props: {
    cols: [String, Number]
  },
  computed: {
    colClasses() {
      return 'px-4 flex-1';
    }
  }
};
</script>
