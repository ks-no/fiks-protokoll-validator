<template>
  <router-link :to="to" class="text-blue-600 hover:text-blue-800 underline">
    <slot></slot>
  </router-link>
</template>

<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

interface Props {
  to: RouteLocationRaw
}

defineProps<Props>()
</script>
