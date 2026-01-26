<template>
  <router-link :to="to" class="text-blue-600 hover:text-blue-800 underline">
    <slot></slot>
  </router-link>
</template>

<script>
export default {
  name: 'BLink',
  props: {
    to: [String, Object]
  }
};
</script>
