<template>
  <div id="start-page">
    <div class="my-10">
      <div class="w-full flex flex-col">
        <div class="float-left w-[70%]">
          <h3 class="text-xl font-semibold">
            Sist utførte testsesjon:
          </h3>
        </div>
        <div v-if="lastTestUrl">
          <a
            :href="lastTestUrl"
            class="mt-2 inline-flex items-center text-blue-600 hover:text-blue-800"
          >
            <svg
              class="w-5 h-5 mr-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 19l3 3m0 0l3-3m-3 3V10"
              />
            </svg>
            Din testsesjon utført - {{ lastTestDateTime }} ({{ sessionId }})
          </a>
        </div>
        <div
          v-else
          class="mt-2 text-gray-500"
        >
          Ingen tidligere testsesjon funnet
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { formatDate } from '@/composables/dateFormat'

const lastTestUrl = ref<string | null>(null)
const createdAt = ref<string | null>(null)

onMounted(() => {
  lastTestUrl.value = localStorage.getItem('validatorLastTest')
  createdAt.value = localStorage.getItem('createdAt')
})

const lastTestDateTime = computed(() => {
  if (!createdAt.value) return ''
  return formatDate(createdAt.value)
})

const sessionId = computed(() => {
  if (!lastTestUrl.value) return ''
  try {
    const url = new URL(lastTestUrl.value, window.location.origin)
    const pathParts = url.pathname.split('/')
    const testSessionIndex = pathParts.findIndex(part => part === 'TestSession')
    if (testSessionIndex !== -1 && pathParts[testSessionIndex + 1]) {
      return pathParts[testSessionIndex + 1]
    }
    return ''
  } catch {
    const parts = lastTestUrl.value.split('TestSession/')
    return parts[1] || ''
  }
})
</script>
