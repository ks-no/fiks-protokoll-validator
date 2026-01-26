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
    <BCollapse :visible="!isCollapsed" :id="'collapse-' + testCase.operation + testCase.situation">
      <BContainer fluid>
        <BRow class="mb-6">
          <BCol cols="2"><strong class="float-right">Sendt: </strong></BCol>
          <BCol>{{ formatDateTime(sentAt) }}</BCol>
        </BRow>
        <div v-if="testCase.fiksResponseTests && testCase.fiksResponseTests.length > 0">
          <BRow class="mb-1.5">
            <BCol cols="2">
              <strong class="float-right">Testspørringer:</strong>
            </BCol>
            <BCol>
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
            </BCol>
          </BRow>
        </div>
      </BContainer>
      <BCard>
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
      </BCard>
      <BCard>
        <h6 class="font-semibold">Testresultat</h6>
        <div v-if="validState === 'notValidated'" class="flex items-center gap-2">
          <BIconExclamationCircleFill
            :class="'validState ' + validState"
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
            <BIconExclamationCircleFill
              :class="'validState ' + validState"
              title="Ugyldig"
            />
            {{ error }}
          </label>
        </div>
      </BCard>
    </BCollapse>
  </li>
</template>

<script setup lang="ts">
import { ref, onBeforeMount } from 'vue'
import { useDateFormat } from '@/composables/useDateFormat'
import TestCase from './TestCase.vue'
import Response from './Response.vue'
import BContainer from '@/components/ui/BContainer.vue'
import BRow from '@/components/ui/BRow.vue'
import BCol from '@/components/ui/BCol.vue'
import BCollapse from '@/components/ui/BCollapse.vue'
import BCard from '@/components/ui/BCard.vue'
import BIconExclamationCircleFill from '@/components/ui/icons/BIconExclamationCircleFill.vue'

type ValidationState = 'valid' | 'invalid' | 'notValidated'

interface FiksResponseTest {
  id: string
  payloadQuery: string
  valueType: number
  expectedValue: string
}

interface FiksPayload {
  id: string
  filename: string
  payload?: string
}

interface FiksResponse {
  id: string
  receivedAt: string
  type: string
  fiksPayloads: FiksPayload[]
  payloadContent?: string
}

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

<style scoped>
svg.validState {
  font-size: 24px;
  margin-right: 6px;
}

svg.notValidated {
  color: rgb(231, 181, 42);
}

svg.invalid {
  color: #cc3333;
}
</style>
