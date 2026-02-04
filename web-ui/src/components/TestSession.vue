<template>
  <div class="max-w-7xl mx-auto px-4 py-6">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">
        Test Resultater
      </h1>
      <p class="text-gray-600">
        Session ID: {{ route.params.testSessionId }}
      </p>
    </div>

    <div
      v-if="!fetchError"
      class="mb-8 bg-blue-50 border border-blue-200 rounded-lg p-6"
    >
      <label
        for="session-url"
        class="block text-sm font-semibold text-gray-700 mb-3"
      >
        Adresse til denne testen:
      </label>
      <div class="flex gap-3">
        <input
          id="session-url"
          ref="sessionUrlInput"
          v-model="sessionUrl"
          type="text"
          class="flex-1 px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          readonly
        >
        <UiButton
          variant="primary"
          class="px-6 py-2 whitespace-nowrap"
          @click="copyURL"
        >
          {{ copied ? 'Kopiert!' : 'Kopier' }}
        </UiButton>
      </div>
    </div>

    <!-- Error Alert -->
    <div
      v-if="fetchError"
      class="mb-8"
    >
      <UiAlert
        v-model="fetchError"
        variant="danger"
        dismissible
        class="text-base"
      >
        <p
          v-if="requestErrorStatusCode === 404"
          class="font-semibold mb-2"
        >
          Vi kunne ikke finne din test med SessionID: {{ route.params.testSessionId }}
        </p>
        <p
          v-else
          class="font-semibold mb-2"
        >
          Noe gikk galt med SessionID: {{ route.params.testSessionId }}
        </p>
        <p class="text-sm">
          Statuskode: {{ requestErrorStatusCode }}
        </p>
        <p
          v-if="requestErrorStatusCode === 500"
          class="text-sm"
        >
          {{ requestErrorMessage }}
        </p>
        <p
          v-else
          class="text-sm"
        >
          {{ (requestErrorMessage as ErrorMessage)?.title }}
        </p>
      </UiAlert>
    </div>

    <!-- Loading Spinner -->
    <div
      v-if="loading"
      class="flex items-center justify-center py-12"
    >
      <UiSpinner label="Laster..." />
      <span class="ml-3 text-gray-600">Laster testresultater...</span>
    </div>

    <!-- Update Tests Section -->
    <div
      v-if="showUpdateButton"
      class="mb-8 bg-white border border-gray-200 rounded-lg p-6"
    >
      <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-6">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 mb-3">
            Statusforklaring
          </h3>
          <div class="space-y-2 text-sm text-gray-700">
            <p class="flex items-center gap-2">
              <font-awesome-icon
                icon="fa-solid fa-circle-exclamation"
                class="validState invalid"
                title="Ugyldig"
              />
              <span><strong>Ugyldig:</strong> Testen har feil eller mangler</span>
            </p>
            <p class="flex items-center gap-2">
              <font-awesome-icon
                icon="fa-solid fa-circle-exclamation"
                class="validState notValidated"
                title="Ikke validert"
              />
              <span><strong>Ikke validert:</strong> Har ikke mottatt svar</span>
            </p>
          </div>
          <p class="mt-4 text-sm text-gray-600">
            Oppdater testene for å validere på nytt ved rød eller gul status
          </p>
        </div>

        <div class="flex-shrink-0">
          <UiButton
            variant="primary"
            class="px-6 py-2.5 text-base font-medium"
            @click="fetchTestSession"
          >
            Oppdater tester
          </UiButton>
        </div>
      </div>
    </div>

    <!-- Test Results -->
    <div v-if="testSession && testSession.fiksRequests">
      <h2 class="text-2xl font-bold text-gray-900 mb-6">
        Test Resultater
      </h2>
      <ul class="divide-y divide-gray-200">
        <Request
          v-for="request in testSession.fiksRequests"
          :key="request.messageGuid"
          :collapse-id="request.messageGuid"
          :has-run="true"
          :sent-at="request.sentAt"
          :responses="request.fiksResponses"
          :test-case="request.testCase"
          :custom-payload-filename="request.customPayloadFile?.filename"
          :is-validated="request.isFiksResponseValidated"
          :validation-errors="request.fiksResponseValidationErrors"
          :test-session-id="testSession.id"
        />
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useApi } from '@/composables/useApi'
import { useClipboard } from '@/composables/useClipboard'
import Request from './Request.vue'
import type { TestSession as TestSessionData } from '@/types'
import {UiAlert, UiButton, UiSpinner} from "@/components/ui";

interface ErrorMessage {
  title?: string
}

const route = useRoute()
const { copy, copied } = useClipboard()
const api = useApi<TestSessionData>()

const testSession = ref<TestSessionData | null>(null)
const loading = ref(false)
const fetchError = ref(false)
const sessionUrl = ref(window.location.href)
const requestErrorStatusCode = ref<number | null>(null)
const requestErrorMessage = ref<string | ErrorMessage>('')

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
    const data = await api.get(`/api/TestSessions/${testSessionId}`)
    testSession.value = {
      ...data,
      fiksRequests: sortRequests(data.fiksRequests)
    }
    localStorage.setItem('validatorLastTest', sessionUrl.value)
    localStorage.setItem('createdAt', data.createdAt)
  } catch (err) {
    const error = err as { status: number; data?: unknown }
    requestErrorStatusCode.value = error.status
    requestErrorMessage.value = error.data as string | ErrorMessage
    fetchError.value = true
  } finally {
    loading.value = false
  }
}

function sortRequests(requests: TestSessionData['fiksRequests']): TestSessionData['fiksRequests'] {
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
