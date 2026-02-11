<template>
  <div>
    <div class="flex items-center gap-3">
      <div class="flex-1">
        <div
          class="cursor-pointer inline-flex items-center gap-2 hover:text-blue-600 transition-colors"
          role="button"
          tabindex="0"
          @click="handleToggle"
          @keyup.enter="handleToggle"
          @keyup.space="handleToggle"
        >
          <PhCaretDown
            v-if="isNotCollapsed"
            class="text-sm text-gray-400"
            weight="bold"
          />
          <PhCaretRight
            v-else
            class="text-sm text-gray-400"
            weight="bold"
          />
          <span class="font-medium">{{ testName }}</span>
        </div>
      </div>
      <div class="flex items-center gap-3">
        <UiCheckbox
          v-if="!hasRun && supported"
          switch
          :value="testId"
        />
        <PhWarningCircle
          v-if="validState === 'invalid'"
          :class="'validState ' + validState"
          weight="fill"
          title="Ugyldig"
        />
        <PhCheckCircle
          v-else-if="validState === 'valid'"
          :class="'validState ' + validState"
          weight="fill"
          title="Gyldig"
        />
        <PhWarningCircle
          v-else-if="validState === 'notValidated'"
          :class="'validState ' + validState"
          weight="fill"
          title="Ikke validert"
        />
      </div>
    </div>
    <div
      v-show="isNotCollapsed"
      :id="'collapse-' + operation + situation"
      class="mt-3 ml-6"
    >
      <div class="bg-gray-50 rounded-md p-4 text-sm grid grid-cols-[auto_1fr] gap-x-4 gap-y-2">
        <strong class="text-gray-600">Beskrivelse:</strong>
        <span class="text-gray-800">{{ description }}</span>

        <strong class="text-gray-600">ID:</strong>
        <span class="text-gray-800 font-mono">{{ operation + situation }}</span>

        <strong class="text-gray-600">Meldingstype:</strong>
        <span class="text-gray-800 font-mono text-xs">{{ messageType }}</span>

        <strong class="text-gray-600">Meldingsinnhold:</strong>
        <span>
          <TestCasePayloadFile
            :test-id="testId"
            :test-name="testName"
            :file-name="payloadFileName"
            :operation="operation"
            :situation="situation"
            :protocol="protocol"
            :test-session-id="testSessionId"
            :has-run="hasRun"
          />
        </span>

        <template v-if="payloadAttachmentFileNames">
          <strong class="text-gray-600">Vedlegg:</strong>
          <div class="space-y-1">
            <TestCasePayloadFile
              v-for="attachmentFileName in payloadAttachmentFileNames.split(';')"
              :key="attachmentFileName"
              :test-id="testId"
              :test-name="testName"
              :operation="operation"
              :situation="situation"
              :protocol="protocol"
              :file-name="attachmentFileName"
              :is-attachment="true"
              :test-session-id="testSessionId"
              :has-run="hasRun"
            />
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { PhCaretRight, PhCaretDown, PhWarningCircle, PhCheckCircle } from '@phosphor-icons/vue'
import TestCasePayloadFile from './TestCasePayloadFile.vue'
import type { ValidationState } from '@/types'

interface Props {
  testName: string
  testId: string
  messageType: string
  description: string
  testStep: string
  operation: string
  expectedResult: string
  situation: string
  payloadFileName?: string
  payloadAttachmentFileNames?: string
  supported?: boolean
  hasRun?: boolean
  validState?: ValidationState
  isCollapsed?: boolean
  protocol: string
  testSessionId?: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'toggle-collapse': []
}>()

const isNotCollapsed = ref(!(props.isCollapsed ?? true))

function handleToggle() {
  isNotCollapsed.value = !isNotCollapsed.value
  emit('toggle-collapse')
}
</script>
