<template>
  <li class="py-2 list-none">
    <div
      class="cursor-pointer inline-flex items-center gap-2 hover:text-blue-600 transition-colors"
      role="button"
      tabindex="0"
      @click="isCollapsed = !isCollapsed"
      @keyup.enter="isCollapsed = !isCollapsed"
      @keyup.space="isCollapsed = !isCollapsed"
    >
      <font-awesome-icon
        :icon="isCollapsed ? 'fa-solid fa-chevron-right' : 'fa-solid fa-chevron-down'"
        class="text-xs text-gray-400"
      />
      <span class="font-medium text-sm text-gray-700">{{ props.messageType }}</span>
    </div>
    <div
      v-show="!isCollapsed"
      :id="'collapse-' + props.collapseId"
      class="ml-5 mt-2 pl-3 border-l border-gray-200 text-sm"
    >
      <p class="text-gray-600 mb-1">
        <strong>Mottatt:</strong> {{ formatDateTime(props.receivedAt) }}
      </p>
      <div
        v-for="payload in (props.payloads ?? [])"
        :key="payload.filename"
      >
        <p
          v-if="payload"
          class="text-gray-600"
        >
          <strong>Innhold:</strong>
          <PayloadFile
            :file-name="payload.filename"
            :content="payload.payload"
          />
        </p>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { formatDateTime } from '@/composables/dateFormat'
import PayloadFile from './PayloadFile.vue'
import type { FiksPayload } from '@/types'

interface Props {
  collapseId: string
  receivedAt?: string
  messageType?: string
  payloads?: FiksPayload[]
}

const props = defineProps<Props>()

const isCollapsed = ref(true)
</script>
