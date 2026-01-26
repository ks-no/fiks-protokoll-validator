<template>
  <div :class="colClasses">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type ColSize = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 'auto'

interface Props {
  cols?: ColSize | string | number
  sm?: ColSize | string | number
  md?: ColSize | string | number
  lg?: ColSize | string | number
  xl?: ColSize | string | number
}

const props = defineProps<Props>()

const colClasses = computed(() => {
  const classes = ['px-4']

  // If no size specified, use flex-1
  if (!props.cols && !props.sm && !props.md && !props.lg && !props.xl) {
    classes.push('flex-1')
  } else {
    // Apply responsive width classes based on Tailwind's grid system
    if (props.cols) {
      const width = getWidthClass(props.cols)
      if (width) classes.push(width)
    }
    if (props.sm) {
      const width = getWidthClass(props.sm, 'sm')
      if (width) classes.push(width)
    }
    if (props.md) {
      const width = getWidthClass(props.md, 'md')
      if (width) classes.push(width)
    }
    if (props.lg) {
      const width = getWidthClass(props.lg, 'lg')
      if (width) classes.push(width)
    }
    if (props.xl) {
      const width = getWidthClass(props.xl, 'xl')
      if (width) classes.push(width)
    }
  }

  return classes.join(' ')
})

function getWidthClass(size: ColSize | string | number, prefix?: string): string {
  const numSize = typeof size === 'string' ? parseInt(size, 10) : size
  if (size === 'auto' || isNaN(numSize as number)) {
    return prefix ? `${prefix}:w-auto` : 'w-auto'
  }

  // Map column sizes to Tailwind width percentages
  const widthMap: Record<number, string> = {
    1: 'w-1/12',
    2: 'w-2/12',
    3: 'w-3/12',
    4: 'w-4/12',
    5: 'w-5/12',
    6: 'w-6/12',
    7: 'w-7/12',
    8: 'w-8/12',
    9: 'w-9/12',
    10: 'w-10/12',
    11: 'w-11/12',
    12: 'w-full'
  }

  const widthClass = widthMap[numSize as number] || 'w-full'
  return prefix ? `${prefix}:${widthClass}` : widthClass
}
</script>
