<template>
  <div
    v-show="visible"
    :id="id"
    class="overflow-hidden transition-all duration-300"
  >
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
interface Props {
  id?: string
  visible?: boolean
}

withDefaults(defineProps<Props>(), {
  visible: false
})
</script>
