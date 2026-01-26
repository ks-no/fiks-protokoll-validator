<template>
  <div 
    v-show="visible"
    :id="id"
    class="overflow-hidden transition-all duration-300"
  >
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'BCollapse',
  props: {
    id: String,
    visible: Boolean
  }
};
</script>
