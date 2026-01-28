<template>
  <div
    v-if="fileName !== null"
    class="mt-2"
  >
    <form enctype="multipart/form-data">
      <div class="flex items-center gap-2">
        <div class="flex-1">
          <label
            class="flex items-center justify-center px-4 py-2 bg-white border border-gray-300 rounded-lg cursor-pointer hover:bg-gray-50"
          >
            <span class="text-gray-600 text-sm truncate">{{ fileUploadText }}</span>
            <input
              id="payloadUpload"
              ref="fileInput"
              type="file"
              class="hidden"
              @change="handleFileUpload"
            >
          </label>
        </div>
        <button
          type="button"
          :disabled="!file"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg text-sm font-medium flex items-center gap-2"
          title="Last opp egen melding"
          @click="submitFile"
        >
          <span>Last opp</span>
        </button>
        <div class="w-8">
          <span
            v-if="fileUploadSuccess"
            class="text-green-600 text-2xl"
          >
            <font-awesome-icon icon="check" />
          </span>
        </div>
      </div>
      <p
        v-if="fileUploadError"
        class="text-red-600 text-sm mt-1"
      >
        {{ fileUploadError }}
      </p>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useApi } from '@/composables/useApi'

interface Props {
  testId?: string
  fileName?: string
  protocol?: string
}

const props = defineProps<Props>()

const fileInput = ref<HTMLInputElement | null>(null)
const file = ref<File | null>(null)
const fileUploadText = ref('Velg egendefinert melding')
const fileUploadSuccess = ref(false)
const fileUploadError = ref<string | null>(null)

function handleFileUpload() {
  if (fileInput.value?.files?.[0]) {
    file.value = fileInput.value.files[0]
    fileUploadText.value = fileInput.value.files[0].name
  }
}

async function submitFile() {
  if (!file.value) return

  const formData = new FormData()
  formData.append('file', file.value)

  const api = useApi()
  try {
    fileUploadError.value = null
    await api.post(`/api/TestCasePayloadFiles/${props.testId}/payload`, formData)
    fileUploadSuccess.value = true
  } catch (err) {
    const error = err as { message?: string }
    fileUploadError.value = error.message ?? 'Opplasting feilet'
    fileUploadSuccess.value = false
  }
}
</script>
