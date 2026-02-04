<template>
  <li class="py-4 list-none">
    <TestCase
      :test-id="testCase.testId"
      :test-name="testCase.testName"
      :message-type="testCase.messageType"
      :payload-file-name="customPayloadFilename ?? testCase.payloadFileName"
      :payload-attachment-file-names="testCase.payloadAttachmentFileNames"
      :description="testCase.description"
      :expected-result="testCase.expectedResult"
      :situation="testCase.situation"
      :operation="testCase.operation"
      :test-step="testCase.testStep"
      :protocol="testCase.protocol"
      :has-run="hasRun"
      :valid-state="validState"
      :is-collapsed="isCollapsed"
      :test-session-id="testSessionId"
      @toggle-collapse="isCollapsed = !isCollapsed"
    />
    <div
      v-show="!isCollapsed"
      :id="'collapse-' + testCase.operation + testCase.situation"
      class="ml-6 mt-4 pl-4 border-l-2 border-gray-200"
    >
      <div class="grid grid-cols-[auto_1fr] gap-x-4 gap-y-2 text-sm mb-4">
        <strong class="text-gray-600">Sendt:</strong>
        <span>{{ formatDateTime(sentAt) }}</span>

        <template v-if="testCase.fiksResponseTests && testCase.fiksResponseTests.length > 0">
          <strong class="text-gray-600">Testspørringer:</strong>
          <div class="space-y-1">
            <span
              v-for="fiksResponseTest in testCase.fiksResponseTests"
              :key="fiksResponseTest.id"
              class="block text-gray-700 font-mono text-xs"
            >
              {{
                testCase.protocol !== 'no.ks.fiks.politisk.behandling.klient.v1'
                  ? fiksResponseTest.payloadQuery +
                    (fiksResponseTest.valueType === 0
                      ? '/text(' + fiksResponseTest.expectedValue + ')'
                      : "[@xsi:type='" + fiksResponseTest.expectedValue + "']")
                  : fiksResponseTest.payloadQuery +
                    (fiksResponseTest.valueType === 0
                      ? '/forventet verdi(' + fiksResponseTest.expectedValue + ')'
                      : "[key='" + fiksResponseTest.expectedValue + "']")
              }}
            </span>
          </div>
        </template>
      </div>

      <div class="mb-4">
        <h6 class="font-semibold text-gray-800 mb-2">
          Svarmeldinger
        </h6>
        <div
          v-if="validState === 'notValidated'"
          class="text-gray-500 text-sm"
        >
          Ingen svarmeldinger funnet..
        </div>
        <ul class="space-y-2">
          <Response
            v-for="response in responses"
            :key="response.id"
            :collapse-id="'collapse-' + response.id"
            :received-at="response.receivedAt"
            :message-type="response.type"
            :payloads="response.fiksPayloads"
          />
        </ul>
      </div>

      <div class="pt-4 border-t border-gray-100">
        <h6 class="font-semibold text-gray-800 mb-2">
          Testresultat
        </h6>
        <div
          v-if="validState === 'notValidated'"
          class="flex items-center gap-2 text-sm"
        >
          <font-awesome-icon
            icon="fa-solid fa-circle-exclamation"
            class="validState notValidated"
            title="Ikke validert"
          />
          <span class="text-gray-600">Validering er ikke utført</span>
        </div>
        <div
          v-else-if="validState === 'valid'"
          class="text-green-600 text-sm"
        >
          Validering utført uten feil!
        </div>
        <div
          v-else-if="validState === 'invalid'"
          class="space-y-1"
        >
          <div
            v-for="error in validationErrors"
            :key="error"
            class="flex items-center gap-2 text-sm"
          >
            <font-awesome-icon
              icon="fa-solid fa-circle-exclamation"
              class="validState invalid"
              title="Ugyldig"
            />
            <span class="text-red-700">{{ error }}</span>
          </div>
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { formatDateTime } from '@/composables/dateFormat'
import TestCase from './TestCase.vue'
import Response from './Response.vue'
import type { FiksResponseTest, FiksResponse, ValidationState } from '@/types'

interface TestCaseData {
  testId: string
  testName: string
  messageType: string
  description: string
  testStep: string
  operation: string
  situation: string
  expectedResult: string
  payloadFileName?: string
  payloadAttachmentFileNames?: string
  protocol: string
  fiksResponseTests?: FiksResponseTest[]
}

interface Props {
  collapseId: string
  hasRun: boolean
  sentAt: string
  testCase: TestCaseData
  responses: FiksResponse[]
  isValidated?: boolean
  validationErrors?: string[]
  testSessionId?: string
  customPayloadFilename?: string
}

const props = defineProps<Props>()

const isCollapsed = ref(true)

const validState = computed<ValidationState>(() => {
  if (!props.isValidated) return 'notValidated'
  return (props.validationErrors?.length ?? 0) > 0 ? 'invalid' : 'valid'
})
</script>
