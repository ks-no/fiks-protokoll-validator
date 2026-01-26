<template>
  <div class="flex flex-wrap" :class="rowClasses">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  noGutters?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  noGutters: false
})

const rowClasses = computed(() => {
  return props.noGutters ? '' : '-mx-4'
})
</script>
