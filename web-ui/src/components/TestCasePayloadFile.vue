<template>
  <div>
    <span>
      <PayloadFile
        :fileName="fileName"
        :testId="testId"
        :protocol="protocol"
        :content="payloadFileContent"
        :fileUrl="isAttachment ? attachmentUrl : payloadUrl"
        @get-content="(isText: boolean) => getContent(isText)"
      />
    </span>
    <span v-if="!hasRun && !isAttachment">
      <PayloadFileUpload
        :fileName="fileName"
        :testId="testId"
        :protocol="protocol"
      />
    </span>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useApi } from '@/composables/useApi'
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

const props = withDefaults(defineProps<Props>(), {
  isAttachment: false,
  hasRun: false
})

const payloadFileContent = ref<string | undefined>(undefined)
const apiBaseUrl = import.meta.env.VITE_API_URL + '/api/TestCasePayloadFiles'

const payloadUrl = computed(() => {
  return props.hasRun
    ? `${apiBaseUrl}/${props.testSessionId}/${props.testId}/payload`
    : `${apiBaseUrl}/${props.testId}/payload`
})

const attachmentUrl = computed(() => {
  return `${apiBaseUrl}/${props.testId}/Attachment/${props.fileName}`
})

async function getContent(isTextContent: boolean) {
  const resourceUrl = props.isAttachment ? attachmentUrl.value : payloadUrl.value
  const api = useApi<string>()

  try {
    const result = await api.get(
      resourceUrl.replace(import.meta.env.VITE_API_URL || '', ''),
      { responseType: isTextContent ? 'text' : 'text' }
    )
    payloadFileContent.value = result
  } catch {
    // Could add error handling here
  }
}
</script>
