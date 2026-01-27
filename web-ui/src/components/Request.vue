<template>
  <li class="border-b border-gray-200 py-4">
    <TestCase
      :testId="testCase.testId"
      :testName="testCase.testName"
      :messageType="testCase.messageType"
      :payloadFileName="customPayloadFilename ?? testCase.payloadFileName"
      :payloadAttachmentFileNames="testCase.payloadAttachmentFileNames"
      :description="testCase.description"
      :expectedResult="testCase.expectedResult"
      :situation="testCase.situation"
      :operation="testCase.operation"
      :testStep="testCase.testStep"
      :protocol="testCase.protocol"
      :hasRun="hasRun"
      :validState="validState"
      :isCollapsed="isCollapsed"
      :testSessionId="testSessionId"
    />
    <div v-show="!isCollapsed" :id="'collapse-' + testCase.operation + testCase.situation">
      <div class="w-full px-4">
        <div class="grid grid-cols-[1fr_2fr] gap-y-1.5 mb-6">
          <strong class="text-right pr-2">Sendt: </strong>
          <span>{{ formatDateTime(sentAt) }}</span>
        </div>
        <div v-if="testCase.fiksResponseTests && testCase.fiksResponseTests.length > 0">
          <div class="grid grid-cols-[1fr_2fr] gap-y-1.5 mb-1.5">
            <strong class="text-right pr-2">Testspørringer:</strong>
            <span>
              <a
                v-for="fiksResponseTest in testCase.fiksResponseTests"
                :key="fiksResponseTest.id"
                class="block"
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
              </a>
            </span>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-lg shadow p-6 mb-8">
        <ul class="space-y-2">
          <h6 class="font-semibold">Svarmeldinger</h6>
          <span v-if="validState === 'notValidated'">Ingen svarmeldinger funnet..</span>
          <Response
            v-for="response in responses"
            :key="response.id"
            :collapseId="'collapse-' + response.id"
            :receivedAt="response.receivedAt"
            :messageType="response.type"
            :payloads="response.fiksPayloads"
            :payloadContent="response.payloadContent"
          />
        </ul>
      </div>
      <div class="bg-white rounded-lg shadow p-6 mb-8">
        <h6 class="font-semibold">Testresultat</h6>
        <div v-if="validState === 'notValidated'" class="flex items-center gap-2">
          <font-awesome-icon
            icon="fa-solid fa-circle-exclamation"
            class="validState notValidated"
            title="Ikke validert"
          />
          <label>Validering er ikke utført</label>
        </div>
        <div v-else-if="validState === 'valid'">
          <label>Validering utført uten feil!</label>
        </div>
        <div v-else-if="validState === 'invalid'">
          <label
            v-for="error in validationErrors"
            :key="error"
            class="flex items-center gap-2"
          >
            <font-awesome-icon
              icon="fa-solid fa-circle-exclamation"
              class="validState invalid"
              title="Ugyldig"
            />
            {{ error }}
          </label>
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { ref, onBeforeMount } from 'vue'
import { useDateFormat } from '@/composables/useDateFormat'
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

const props = withDefaults(defineProps<Props>(), {
  isValidated: false
})

const isCollapsed = ref(true)
const validState = ref<ValidationState>('notValidated')
const { formatDateTime } = useDateFormat()

onBeforeMount(() => {
  if (!props.isValidated) {
    validState.value = 'notValidated'
  } else {
    validState.value =
      props.validationErrors && props.validationErrors.length > 0
        ? 'invalid'
        : 'valid'
  }
})
</script>
