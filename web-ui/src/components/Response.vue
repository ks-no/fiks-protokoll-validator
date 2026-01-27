<template>
  <li class="border-b border-gray-200 py-4">
    <span class="w-full block">
      <div
        class="cursor-pointer"
        @click="isCollapsed = !isCollapsed"
      >
        <strong class="flex items-center gap-2">
          <font-awesome-icon v-if="isCollapsed" icon="fa-solid fa-file-circle-plus" />
          <font-awesome-icon v-if="!isCollapsed" icon="fa-solid fa-file-circle-minus" />
          {{ messageType }}
        </strong>
      </div>
    </span>
    <div v-show="!isCollapsed" :id="'collapse-' + collapseId" class="mt-2">
      <div class="bg-white rounded-lg shadow p-6 mb-8">
        <p><strong>Mottatt: </strong>{{ formatDateTime(receivedAt) }}</p>
        <div
          v-for="payload in payloads"
          :key="payload.filename"
        >
          <p v-if="payload">
            <strong>Innhold: </strong>
            <PayloadFile :fileName="payload.filename" :content="payload.payload" />
          </p>
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useDateFormat } from '@/composables/useDateFormat'
import PayloadFile from './PayloadFile.vue'
import type { FiksPayload } from '@/types'

interface Props {
  collapseId: string
  receivedAt?: string
  messageType?: string
  payloads?: FiksPayload[]
  payloadContent?: string
}

defineProps<Props>()

const isCollapsed = ref(true)
const { formatDateTime } = useDateFormat()
</script>
