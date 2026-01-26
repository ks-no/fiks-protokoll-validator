<template>
  <div class="newTestSession max-w-7xl mx-auto px-4 py-6">
    <h1 class="text-3xl font-bold text-gray-900 mb-6">Ny Test Sesjon</h1>

    <div class="mb-6">
      <label for="account-id" class="block text-sm font-semibold text-gray-700 mb-2">
        FIKS-konto (UUID)
      </label>
      <input
        id="account-id"
        v-model="recipientId"
        type="text"
        class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-base"
        placeholder="f.eks. 76b035a9-2d73-48bf-b758-981195333191"
        aria-label="FIKS-konto UUID"
      />
      <p class="mt-2 text-sm text-gray-600">
        Angi UUID for FIKS-kontoen du vil kjøre tester mot
      </p>
    </div>

    <div class="mb-6">
      <label for="protocol-select" class="block text-sm font-semibold text-gray-700 mb-2">
        FIKS-protokoll
      </label>
      <BFormSelect
        id="protocol-select"
        v-model="selectedProtocol"
        @change="getTestsByProtocol"
        :options="protocolOptions"
        class="w-full md:w-1/2"
      />
    </div>

    <div class="mb-6">
      <BLink
        :to="{
          name: 'newTestSession',
          query: { fikskonto: recipientId, fiksprotocol: selectedProtocol },
        }"
        class="text-blue-600 hover:text-blue-800 underline"
      >
        Direkte lenke
      </BLink>
    </div>

    <div class="mb-8">
      <BFormGroup v-if="!hasRun">
        <div class="bg-white border border-gray-200 rounded-lg p-6 mb-4">
          <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div class="flex-1">
              <h3 class="text-2xl font-bold text-gray-900 mb-3">
                Tester
              </h3>
              <BFormCheckbox
                v-show="!hasRun"
                id="switch_supported"
                v-model="showNotSupportedTests"
                aria-describedby="testCases"
                aria-controls="testCases"
                @change="toggleAllSupportedTests"
              >
                Vis tester som ikke er implementert
              </BFormCheckbox>
            </div>

            <div class="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center">
              <BFormCheckbox
                v-show="!hasRun"
                id="switch_selectAllTests"
                switch
                v-model="allTestsSelected"
                aria-describedby="testCases"
                aria-controls="testCases"
                @change="toggleAll"
                class="text-base"
              >
                {{ allTestsSelected ? 'Velg ingen' : 'Velg alle' }}
              </BFormCheckbox>

              <BButton
                variant="primary"
                @click="runSelectedTests"
                v-if="!hasRun || running"
                :disabled="running || !fiksAccountPresent || selectedTests.length === 0"
                class="px-6 py-2.5 text-base font-medium"
              >
                <BSpinner v-if="running" small class="mr-2"></BSpinner>
                {{ running ? 'Kjører...' : `Kjør valgte tester (${selectedTests.length})` }}
              </BButton>
            </div>
          </div>
        </div>

        <BAlert v-model="showRequestError" variant="danger" dismissible class="mb-4">
          <p class="font-semibold">Testing feilet med statuskode {{ requestErrorStatusCode }}</p>
          <p class="text-sm mt-1">{{ requestErrorMessage }}</p>
        </BAlert>

        <div v-if="loading && !running" class="flex items-center justify-center py-8">
          <BSpinner label="Laster tester..."></BSpinner>
          <span class="ml-3 text-gray-600">Laster tester...</span>
        </div>

        <BFormCheckboxGroup
          id="test-list-all"
          v-model="selectedTests"
          name="test-list-all"
          stacked
        >
          <TestCase
            v-for="testCase in computedTestCases"
            :key="testCase.testId"
            :testId="testCase.testId"
            :testName="testCase.testName"
            :messageType="testCase.messageType"
            :payloadFileName="testCase.payloadFileName"
            :payloadAttachmentFileNames="testCase.payloadAttachmentFileNames"
            :description="testCase.description"
            :testStep="testCase.testStep"
            :operation="testCase.operation"
            :situation="testCase.situation"
            :expectedResult="testCase.expectedResult"
            :supported="testCase.supported"
            :protocol="testCase.protocol"
            :hasRun="hasRun"
            :isCollapsed="true"
          />
        </BFormCheckboxGroup>
      </BFormGroup>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import TestCase from './TestCase.vue'
import BFormSelect from '@/components/ui/BFormSelect.vue'
import BLink from '@/components/ui/BLink.vue'
import BFormGroup from '@/components/ui/BFormGroup.vue'
import BFormCheckbox from '@/components/ui/BFormCheckbox.vue'
import BButton from '@/components/ui/BButton.vue'
import BSpinner from '@/components/ui/BSpinner.vue'
import BAlert from '@/components/ui/BAlert.vue'
import BFormCheckboxGroup from '@/components/ui/BFormCheckboxGroup.vue'

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
  supported: boolean
  protocol: string
}

interface ProtocolOption {
  value: string
  text: string
}

const route = useRoute()
const router = useRouter()

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
const fiksAccountPresent = ref(false)
const allTestsSelected = ref(false)
const showNotSupportedTests = ref(false)
const showRequestError = ref(false)
const requestErrorStatusCode = ref(0)
const requestErrorMessage = ref('')
const selectedProtocol = ref(initialProtocol)

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
    const response = await axios.get(
      `${import.meta.env.VITE_API_URL}/api/TestCases/Protocol/${selectedProtocol.value}`,
      { withCredentials: true }
    )
    testCases.value = response.data
  } catch {
    // Could add error handling
  } finally {
    loading.value = false
  }
}

async function runSelectedTests() {
  if (selectedTests.value.length === 0) return

  running.value = true
  showRequestError.value = false

  try {
    const response = await axios.post(
      `${import.meta.env.VITE_API_URL}/api/TestSessions`,
      {
        recipientId: recipientId.value,
        selectedTestCaseIds: selectedTests.value,
        protocol: selectedProtocol.value
      },
      { withCredentials: true }
    )

    if (response.status === 201) {
      hasRun.value = true
      router.push({ path: `/TestSession/${response.data.id}` })
    }
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      requestErrorStatusCode.value = error.response.status
      requestErrorMessage.value = error.response.data
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

function toggleAllSupportedTests(checked: boolean) {
  showNotSupportedTests.value = checked
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

watch(recipientId, (newVal) => {
  fiksAccountPresent.value = newVal.length > 0
})

watch(selectedProtocol, () => {
  selectedTests.value = []
})

onMounted(() => {
  recipientId.value = (route.query.fikskonto as string) || ''
  if (route.query.fikskonto) {
    fiksAccountPresent.value = true
    if (selectedProtocol.value && selectedProtocol.value !== 'ingen') {
      getTestsByProtocol()
    }
  }
})
</script>
