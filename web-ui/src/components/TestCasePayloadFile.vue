<template>
  <div>
    <span>
      <PayloadFile
        :file-name="fileName"
        :test-id="testId"
        :protocol="protocol"
        :content="payloadFileContent"
        :file-url="isAttachment ? attachmentUrl : payloadUrl"
        @get-content="(isText: boolean) => getContent(isText)"
      />
    </span>
    <span
      v-if="contentError"
      class="text-red-600 text-sm"
    >{{ contentError }}</span>
    <span v-if="!hasRun && !isAttachment">
      <PayloadFileUpload
        :file-name="fileName"
        :test-id="testId"
        :protocol="protocol"
      />
    </span>
  </div>
</template>

<script setup lang="ts">
import {computed, ref} from 'vue'
import {useApi} from '@/composables/useApi'
import PayloadFile from './PayloadFile.vue'
import PayloadFileUpload from '@/components/PayloadFileUpload.vue'

interface Props {
  fileName?: string
  isAttachment?: boolean
  operation?: string
  situation?: string
  protocol?: string
  testName: string
  testId: string
  hasRun?: boolean
  testSessionId?: string
}

const props = defineProps<Props>()

const payloadFileContent = ref<string | undefined>(undefined)
const contentError = ref<string | null>(null)
const apiBaseUrl = import.meta.env.VITE_API_URL + '/api/TestCasePayloadFiles'

const payloadUrl = computed(() => {
  return props.hasRun
    ? `${apiBaseUrl}/${props.testSessionId}/${props.testId}/payload`
    : `${apiBaseUrl}/${props.testId}/payload`
})

const isAttachment = computed(() => props.isAttachment ?? false)

const attachmentUrl = computed(() => {
  return `${apiBaseUrl}/${props.testId}/Attachment/${props.fileName}`
})

async function getContent(isTextContent: boolean) {
  const resourceUrl = isAttachment.value ? attachmentUrl.value : payloadUrl.value
  const api = useApi<string>()

  try {
    contentError.value = null
    payloadFileContent.value = await api.get(
        resourceUrl.replace(import.meta.env.VITE_API_URL || '', ''),
        {responseType: isTextContent ? 'text' : 'blob'}
    )
  } catch (err) {
    const error = err as { message?: string }
    contentError.value = error.message ?? 'Kunne ikke laste innhold'
  }
}
</script>
