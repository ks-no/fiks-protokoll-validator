<template>
  <Teleport to="body">
    <div
      v-if="modelValue"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      @click.self="handleBackdropClick"
    >
      <div :class="modalClasses">
        <!-- Header -->
        <div
          v-if="title"
          class="flex-shrink-0 flex items-center justify-between px-6 py-4 border-b border-gray-200"
        >
          <h3 class="text-lg font-semibold text-gray-900">
            {{ title }}
          </h3>
          <button
            class="text-gray-400 hover:text-gray-600 text-xl leading-none"
            aria-label="Close"
            @click="close"
          >
            &times;
          </button>
        </div>

        <!-- Body -->
        <div class="p-6 overflow-y-auto flex-1 min-h-0">
          <slot />
        </div>

        <!-- Footer -->
        <div
          v-if="!okOnly || $slots.footer"
          class="flex-shrink-0 flex justify-end gap-3 px-6 py-4 border-t border-gray-200"
        >
          <slot name="footer">
            <button
              v-if="!okOnly"
              class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded font-medium transition-colors"
              @click="close"
            >
              {{ cancelTitle }}
            </button>
            <button
              :class="okButtonClasses"
              @click="handleOk"
            >
              {{ okTitle }}
            </button>
          </slot>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'

type ModalSize = 'sm' | 'md' | 'lg' | 'xl'
type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'success' | 'warning'

interface Props {
  modelValue?: boolean
  title?: string
  size?: ModalSize
  okOnly?: boolean
  okTitle?: string
  okVariant?: ButtonVariant
  cancelTitle?: string
  noCloseOnBackdrop?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  okTitle: 'OK',
  okVariant: 'primary',
  cancelTitle: 'Cancel'
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  close: []
  ok: []
}>()

const modalClasses = computed(() => {
  const sizeClasses: Record<ModalSize, string> = {
    sm: 'max-w-sm',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl'
  }
  return `bg-white rounded-lg shadow-xl w-full mx-4 max-h-[90vh] flex flex-col ${sizeClasses[props.size]}`
})

const okButtonClasses = computed(() => {
  const variantClasses: Record<ButtonVariant, string> = {
    primary: 'bg-blue-600 hover:bg-blue-700 text-white',
    secondary: 'bg-gray-600 hover:bg-gray-700 text-white',
    danger: 'bg-red-600 hover:bg-red-700 text-white',
    success: 'bg-green-600 hover:bg-green-700 text-white',
    warning: 'bg-yellow-500 hover:bg-yellow-600 text-white'
  }
  return `px-4 py-2 rounded font-medium transition-colors ${variantClasses[props.okVariant]}`
})

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function handleOk() {
  emit('ok')
  close()
}

function handleBackdropClick() {
  if (!props.noCloseOnBackdrop) {
    close()
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.modelValue) {
    close()
  }
}

// Instance-scoped flag to prevent duplicate listeners per component instance
const listenerAdded = ref(false)

function addKeydownListener() {
  if (!listenerAdded.value) {
    document.addEventListener('keydown', handleKeydown)
    listenerAdded.value = true
  }
}

function removeKeydownListener() {
  if (listenerAdded.value) {
    document.removeEventListener('keydown', handleKeydown)
    listenerAdded.value = false
  }
}

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    addKeydownListener()
  } else {
    removeKeydownListener()
  }
})

onMounted(() => {
  if (props.modelValue) {
    addKeydownListener()
  }
})

onUnmounted(() => {
  removeKeydownListener()
})
</script>
