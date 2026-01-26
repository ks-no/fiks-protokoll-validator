<template>
  <li class="border-b border-gray-200 py-4">
    <span class="w-full block">
      <div
        class="cursor-pointer"
        @click="isCollapsed = !isCollapsed"
      >
        <strong class="flex items-center gap-2">
          <BIconFilePlus v-if="isCollapsed" />
          <BIconFileMinus v-if="!isCollapsed" />
          {{ messageType }}
        </strong>
      </div>
    </span>
    <BCollapse :visible="!isCollapsed" :id="'collapse-' + collapseId" class="mt-2">
      <BCard>
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
      </BCard>
    </BCollapse>
  </li>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useDateFormat } from '@/composables/useDateFormat'
import PayloadFile from './PayloadFile.vue'
import BCollapse from '@/components/ui/BCollapse.vue'
import BCard from '@/components/ui/BCard.vue'
import BIconFilePlus from '@/components/ui/icons/BIconFilePlus.vue'
import BIconFileMinus from '@/components/ui/icons/BIconFileMinus.vue'

interface FiksPayload {
  filename: string
  payload?: string
}

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
