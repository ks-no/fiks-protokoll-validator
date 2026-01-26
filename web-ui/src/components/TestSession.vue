<template>
  <div class="max-w-7xl mx-auto px-4 py-6">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">Test Resultater</h1>
      <p class="text-gray-600">Session ID: {{ route.params.testSessionId }}</p>
    </div>

    <!-- Session URL Copy Box -->
    <div v-if="!fetchError" class="mb-8 bg-blue-50 border border-blue-200 rounded-lg p-6">
      <label for="session-url" class="block text-sm font-semibold text-gray-700 mb-3">
        Adresse til denne testen:
      </label>
      <div class="flex gap-3">
        <input
          id="session-url"
          type="text"
          class="flex-1 px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          v-model="sessionUrl"
          readonly
          ref="sessionUrlInput"
        />
        <BButton
          variant="primary"
          @click="copyURL"
          class="px-6 py-2 whitespace-nowrap"
        >
          {{ copied ? 'Kopiert!' : 'Kopier' }}
        </BButton>
      </div>
    </div>

    <!-- Error Alert -->
    <div v-if="fetchError" class="mb-8">
      <BAlert v-model="fetchError" variant="danger" dismissible class="text-base">
        <p v-if="requestErrorStatusCode === 404" class="font-semibold mb-2">
          Vi kunne ikke finne din test med SessionID: {{ route.params.testSessionId }}
        </p>
        <p v-else class="font-semibold mb-2">
          Noe gikk galt med SessionID: {{ route.params.testSessionId }}
        </p>
        <p class="text-sm">Statuskode: {{ requestErrorStatusCode }}</p>
        <p v-if="requestErrorStatusCode === 500" class="text-sm">{{ requestErrorMessage }}</p>
        <p v-else class="text-sm">{{ (requestErrorMessage as ErrorMessage)?.title }}</p>
      </BAlert>
    </div>

    <!-- Loading Spinner -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <BSpinner label="Laster..."></BSpinner>
      <span class="ml-3 text-gray-600">Laster testresultater...</span>
    </div>

    <!-- Update Tests Section -->
    <div v-if="showUpdateButton" class="mb-8 bg-white border border-gray-200 rounded-lg p-6">
      <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-6">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 mb-3">Statusforklaring</h3>
          <div class="space-y-2 text-sm text-gray-700">
            <p class="flex items-center gap-2">
              <BIconExclamationCircleFill class="validState invalid" title="Ugyldig" />
              <span><strong>Ugyldig:</strong> Testen har feil eller mangler</span>
            </p>
            <p class="flex items-center gap-2">
              <BIconExclamationCircleFill class="validState notValidated" title="Ikke validert" />
              <span><strong>Ikke validert:</strong> Har ikke mottatt svar</span>
            </p>
          </div>
          <p class="mt-4 text-sm text-gray-600">
            Oppdater testene for å validere på nytt ved rød eller gul status
          </p>
        </div>

        <div class="flex-shrink-0">
          <BButton
            variant="primary"
            @click="fetchTestSession"
            class="px-6 py-2.5 text-base font-medium"
          >
            Oppdater tester
          </BButton>
        </div>
      </div>
    </div>

    <!-- Test Results -->
    <div v-if="testSession && testSession.fiksRequests" class="space-y-4">
      <h2 class="text-2xl font-bold text-gray-900 mb-4">Test Resultater</h2>
      <Request
        v-for="request in testSession.fiksRequests"
        :key="request.messageGuid"
        :collapseId="request.messageGuid"
        :hasRun="true"
        :sentAt="request.sentAt"
        :responses="request.fiksResponses"
        :testCase="request.testCase"
        :customPayloadFilename="request.customPayloadFile?.filename"
        :isValidated="request.isFiksResponseValidated"
        :validationErrors="request.fiksResponseValidationErrors"
        :testSessionId="testSession.id"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useClipboard } from '@/composables/useClipboard'
import Request from './Request.vue'
import BButton from '@/components/ui/BButton.vue'
import BAlert from '@/components/ui/BAlert.vue'
import BSpinner from '@/components/ui/BSpinner.vue'
import BIconExclamationCircleFill from '@/components/ui/icons/BIconExclamationCircleFill.vue'

interface TestCase {
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
  fiksResponseTests?: Array<{
    id: string
    payloadQuery: string
    valueType: number
    expectedValue: string
  }>
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

interface FiksRequest {
  messageGuid: string
  sentAt: string
  testCase: TestCase
  fiksResponses: FiksResponse[]
  customPayloadFile?: {
    filename: string
    content?: string
  }
  isFiksResponseValidated: boolean
  fiksResponseValidationErrors: string[]
}

interface TestSessionData {
  id: string
  recipientId: string
  protocol: string
  status: string
  createdAt: string
  completedAt?: string
  fiksRequests: FiksRequest[]
}

interface ErrorMessage {
  title?: string
}

const route = useRoute()
const { copy, copied } = useClipboard()

const testSession = ref<TestSessionData | null>(null)
const loading = ref(false)
const fetchError = ref(false)
const sessionUrl = ref(window.location.href)
const requestErrorStatusCode = ref<number | null>(null)
const requestErrorMessage = ref<string | ErrorMessage>('')
const sessionUrlInput = ref<HTMLInputElement | null>(null)

const showUpdateButton = computed(() => {
  if (!testSession.value?.fiksRequests) return false

  return testSession.value.fiksRequests.some(request => {
    if (request.isFiksResponseValidated) {
      return request.fiksResponseValidationErrors.length > 0
    }
    return true
  })
})

async function fetchTestSession() {
  const testSessionId = route.params.testSessionId as string
  testSession.value = null
  loading.value = true
  fetchError.value = false

  try {
    const response = await axios.get(
      `${import.meta.env.VITE_API_URL}/api/TestSessions/${testSessionId}`
    )

    if (response.status === 200) {
      testSession.value = {
        ...response.data,
        fiksRequests: sortRequests(response.data.fiksRequests)
      }
      localStorage.setItem('validatorLastTest', sessionUrl.value)
      localStorage.setItem('createdAt', response.data.createdAt)
    }
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      requestErrorStatusCode.value = error.response.status
      requestErrorMessage.value = error.response.data
    }
    fetchError.value = true
  } finally {
    loading.value = false
  }
}

function sortRequests(requests: FiksRequest[]): FiksRequest[] {
  if (!requests) return []
  return [...requests].sort((a, b) =>
    new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
  )
}

async function copyURL() {
  await copy(sessionUrl.value)
}

onMounted(() => {
  if (route.params.testSessionId) {
    fetchTestSession()
  }
})
</script>

<style scoped>
img {
  margin-top: 50px;
}

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
