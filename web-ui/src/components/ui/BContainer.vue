<template>
  <div :class="containerClasses">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  fluid?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  fluid: false
})

const containerClasses = computed(() => {
  return props.fluid ? 'w-full px-4' : 'container mx-auto px-4'
})
</script>
