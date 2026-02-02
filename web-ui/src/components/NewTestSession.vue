<template>
  <div class="newTestSession max-w-7xl mx-auto px-4 py-6">
    <h1 class="text-3xl font-bold text-gray-900 mb-6">
      Ny testsesjon
    </h1>

    <div class="mb-6">
      <label
        for="account-id"
        class="block text-sm font-semibold text-gray-700 mb-2"
      >
        Konto ID 
      </label>
      <input
        id="account-id"
        v-model="recipientId"
        type="text"
        class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-base"
        placeholder="f.eks. 76b035a9-2d73-48bf-b758-981195333191"
        aria-label="FIKS-IO konto (UUID)"
      >
      <p class="mt-2 text-sm text-gray-600">
        FIKS-IO konto (UUID)
      </p>
    </div>

    <div class="mb-6">
      <label
        for="protocol-select"
        class="block text-sm font-semibold text-gray-700 mb-2"
      >
        Protokoll
      </label>
      <select
        id="protocol-select"
        v-model="selectedProtocol"
        class="w-full md:w-1/2 px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        @change="getTestsByProtocol"
      >
        <option
          v-for="option in protocolOptions"
          :key="option.value"
          :value="option.value"
        >
          {{ option.text }}
        </option>
      </select>
    </div>

    <div class="mb-6">
      <router-link
        :to="{
          name: 'newTestSession',
          query: { fikskonto: recipientId, fiksprotocol: selectedProtocol },
        }"
        class="text-blue-600 hover:text-blue-800 underline"
      >
        Direkte lenke
      </router-link>
    </div>

    <div class="mb-8">
      <div v-if="!hasRun">
        <div class="bg-white border border-gray-200 rounded-lg p-6 mb-4">
          <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div class="flex-1">
              <h3 class="text-2xl font-bold text-gray-900 mb-3">
                Tester
              </h3>
              <UiCheckbox
                v-show="!hasRun"
                id="switch_supported"
                v-model="showNotSupportedTests"
                aria-describedby="testCases"
                aria-controls="testCases"
              >
                Vis tester som ikke er implementert
              </UiCheckbox>
            </div>

            <div class="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center">
              <UiCheckbox
                v-show="!hasRun"
                id="switch_selectAllTests"
                v-model="allTestsSelected"
                switch
                aria-describedby="testCases"
                aria-controls="testCases"
                class="text-base"
                @change="toggleAll"
              >
                {{ allTestsSelected ? 'Velg ingen' : 'Velg alle' }}
              </UiCheckbox>

              <UiButton
                v-if="!hasRun || running"
                variant="primary"
                :disabled="running || !fiksAccountPresent || selectedTests.length === 0"
                class="px-6 py-2.5 text-base font-medium"
                @click="runSelectedTests"
              >
                <UiSpinner
                  v-if="running"
                  small
                  class="mr-2"
                />
                {{ running ? 'Kjører...' : `Kjør valgte tester (${selectedTests.length})` }}
              </UiButton>
            </div>
          </div>
        </div>

        <UiAlert
          v-model="showRequestError"
          variant="danger"
          dismissible
          class="mb-4"
        >
          <p class="font-semibold mr-2">
            {{ requestErrorTitle }}
          </p>
          <p class="text-sm">
            {{ requestErrorMessage }}
          </p>
          <p 
            v-if="requestErrorStatusCode > 0" 
            class="text-xs mt-2 text-gray-600"
          >
            Statuskode: {{ requestErrorStatusCode }}
          </p>
        </UiAlert>

        <div
          v-if="loading && !running"
          class="flex items-center justify-center py-8"
        >
          <UiSpinner label="Laster tester..." />
          <span class="ml-3 text-gray-600">Laster tester...</span>
        </div>

        <UiCheckboxGroup
          id="test-list-all"
          v-model="selectedTests"
          name="test-list-all"
          stacked
        >
          <TestCase
            v-for="testCase in computedTestCases"
            :key="testCase.testId"
            :test-id="testCase.testId"
            :test-name="testCase.testName"
            :message-type="testCase.messageType"
            :payload-file-name="testCase.payloadFileName"
            :payload-attachment-file-names="testCase.payloadAttachmentFileNames"
            :description="testCase.description"
            :test-step="testCase.testStep"
            :operation="testCase.operation"
            :situation="testCase.situation"
            :expected-result="testCase.expectedResult"
            :supported="testCase.supported"
            :protocol="testCase.protocol"
            :has-run="hasRun"
            :is-collapsed="true"
          />
        </UiCheckboxGroup>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '@/composables/useApi'
import TestCase from './TestCase.vue'
import type { TestCase as TestCaseData, ProtocolOption } from '@/types'
import type { CreateTestSessionResponse } from '@/types/api'
import {UiAlert, UiCheckboxGroup, UiSpinner} from "@/components/ui";

const route = useRoute()
const router = useRouter()
const testCaseApi = useApi<TestCaseData[]>()
const sessionApi = useApi<CreateTestSessionResponse>()

const protocolOptions: ProtocolOption[] = [
  { value: 'ingen', text: 'Velg en FIKS-protokoll' },
  { value: 'no.ks.fiks.arkiv.v1', text: 'no.ks.fiks.arkiv.v1' },
  { value: 'no.ks.fiks.plan.v2', text: 'no.ks.fiks.plan.v2' },
  { value: 'no.ks.fiks.matrikkelfoering.v2', text: 'no.ks.fiks.matrikkelfoering.v2' },
  { value: 'no.ks.fiks.saksfaser.v1', text: 'no.ks.fiks.saksfaser.v1' }
]

const qproto = (route.query.fiksprotocol as string) || protocolOptions[0]!.value
const initialProtocol = protocolOptions.find(p => p.value === qproto)?.value || protocolOptions[0]!.value

const testCases = ref<TestCaseData[]>([])
const running = ref(false)
const hasRun = ref(false)
const loading = ref(false)
const recipientId = ref((route.query.fikskonto as string) || '')
const selectedTests = ref<string[]>([])
const fiksAccountPresent = computed(() => recipientId.value.length > 0)
const allTestsSelected = ref(false)
const showNotSupportedTests = ref(false)
const showRequestError = ref(false)
const requestErrorStatusCode = ref(0)
const requestErrorMessage = ref('')
const selectedProtocol = ref(initialProtocol)

const requestErrorTitle = computed(() => {
  if (requestErrorStatusCode.value === 0) {
    return 'Kunne ikke fullføre forespørselen'
  }
  if (requestErrorStatusCode.value === 404) {
    return 'Ressursen ble ikke funnet'
  }
  if (requestErrorStatusCode.value === 400) {
    return 'Ugyldig forespørsel'
  }
  if (requestErrorStatusCode.value >= 500) {
    return 'Serverfeil'
  }
  return `Feil ved testing`
})

const computedTestCases = computed(() => {
  if (selectedProtocol.value === 'ingen') return []
  if (!showNotSupportedTests.value) {
    return testCases.value.filter(tc => tc.supported)
  }
  return testCases.value
})

async function getTestsByProtocol() {
  if (selectedProtocol.value === 'ingen') return

  loading.value = true
  try {
    testCases.value = await testCaseApi.get(`/api/TestCases/Protocol/${selectedProtocol.value}`)
  } catch (err) {
    const error = err as { status: number; data?: unknown; message?: string }
    requestErrorStatusCode.value = error.status
    
    // Handle different error scenarios with user-friendly messages
    if (error.status === 0) {
      // Network error - use the friendly message from useApi
      requestErrorMessage.value = error.message || 'Noe gikk galt. Kontroller nettverkstilkoblingen og prøv igjen.'
    } else if (error.status === 404) {
      requestErrorMessage.value = 'Ressursen ble ikke funnet. Kontroller at protokollen er korrekt.'
    } else if (error.status === 400) {
      const errorData = error.data as string
      // Check if it's the "Ugyldig konto" error from backend
      if (errorData && errorData.includes('Ugyldig konto')) {
        requestErrorMessage.value = 'FIKS-IO kontoen ble ikke funnet. Kontroller at konto-IDen er korrekt.'
      } else {
        requestErrorMessage.value = errorData || 'Ugyldig forespørsel. Kontroller at alle verdier er korrekte.'
      }
    } else {
      requestErrorMessage.value = (error.data as string) || 'Kunne ikke laste tester. Prøv igjen senere.'
    }
    
    showRequestError.value = true
  } finally {
    loading.value = false
  }
}

async function runSelectedTests() {
  if (selectedTests.value.length === 0) return

  running.value = true
  showRequestError.value = false

  try {
    const data = await sessionApi.post('/api/TestSessions', {
      recipientId: recipientId.value,
      selectedTestCaseIds: selectedTests.value,
      protocol: selectedProtocol.value
    })

    hasRun.value = true
    router.push({ path: `/TestSession/${data.id}` })
  } catch (err) {
    const error = err as { status: number; data?: unknown; message?: string }
    requestErrorStatusCode.value = error.status
    
    // Handle different error scenarios with user-friendly messages
    if (error.status === 0) {
      // Network error - use the friendly message from useApi
      requestErrorMessage.value = error.message || 'Noe gikk galt. Kontroller FIKS-IO kontoen og nettverkstilkoblingen.'
    } else if (error.status === 404) {
      requestErrorMessage.value = 'FIKS-IO kontoen ble ikke funnet. Kontroller at konto-IDen er korrekt.'
    } else if (error.status === 400) {
      const errorData = error.data as string
      // Check if it's the "Ugyldig konto" error from backend
      if (errorData && errorData.includes('Ugyldig konto')) {
        requestErrorMessage.value = 'FIKS-IO kontoen ble ikke funnet. Kontroller at konto-IDen er korrekt.'
      } else {
        requestErrorMessage.value = errorData || 'Ugyldig forespørsel. Kontroller at alle verdier er korrekte.'
      }
    } else if (error.status >= 500) {
      const errorData = error.data as string
      requestErrorMessage.value = errorData || 'En serverfeil oppstod. Prøv igjen senere.'
    } else {
      requestErrorMessage.value = (error.data as string) || 'En uventet feil oppstod. Prøv igjen senere.'
    }
    
    showRequestError.value = true
  } finally {
    running.value = false
  }
}

function toggleAll(checked: boolean) {
  if (checked) {
    const tests = testCases.value.filter(
      tc => tc.supported && tc.protocol === selectedProtocol.value
    )
    selectedTests.value = tests.map(t => t.testId)
  } else {
    selectedTests.value = []
  }
}

// Watchers
watch(selectedTests, (newVal) => {
  const supportedCount = testCases.value.filter(
    tc => tc.supported && tc.protocol === selectedProtocol.value
  ).length

  if (newVal.length === 0) {
    allTestsSelected.value = false
  } else if (newVal.length === supportedCount) {
    allTestsSelected.value = true
  } else {
    allTestsSelected.value = false
  }
})

watch(selectedProtocol, () => {
  selectedTests.value = []
})

onMounted(() => {
  recipientId.value = (route.query.fikskonto as string) || ''
  if (route.query.fikskonto && selectedProtocol.value && selectedProtocol.value !== 'ingen') {
    getTestsByProtocol()
  }
})
</script>
