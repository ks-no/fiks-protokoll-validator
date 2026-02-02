<template>
  <li class="border-b border-gray-200 py-4">
    <span class="w-full block">
      <div
        class="cursor-pointer"
        role="button"
        tabindex="0"
        @click="isCollapsed = !isCollapsed"
        @keyup.enter="isCollapsed = !isCollapsed"
        @keyup.space="isCollapsed = !isCollapsed"
      >
        <strong class="flex items-center gap-2">
          <font-awesome-icon
            v-if="isCollapsed"
            icon="fa-solid fa-file-circle-plus"
          />
          <font-awesome-icon
            v-if="!isCollapsed"
            icon="fa-solid fa-file-circle-minus"
          />
          {{ props.messageType }}
        </strong>
      </div>
    </span>
    <div
      v-show="!isCollapsed"
      :id="'collapse-' + props.collapseId"
      class="mt-2"
    >
      <div class="bg-white rounded-lg shadow p-6 mb-8">
        <p><strong>Mottatt: </strong>{{ formatDateTime(props.receivedAt) }}</p>
        <div
          v-for="payload in (props.payloads ?? [])"
          :key="payload.filename"
        >
          <p v-if="payload">
            <strong>Innhold: </strong>
            <PayloadFile
              :file-name="payload.filename"
              :content="payload.payload"
            />
          </p>
        </div>
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
