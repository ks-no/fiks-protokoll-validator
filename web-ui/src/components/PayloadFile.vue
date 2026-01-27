<template>
  <div>
    <span>
      <button
        v-if="fileName !== null"
        class="text-blue-600 hover:text-blue-800 underline bg-transparent border-none cursor-pointer p-0"
        @click="openWindow(fileUrl)"
      >
        {{ fileName }}
      </button>
      <button
        v-if="fileName !== null"
        class="ml-2 px-2 py-1 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded"
        @click="handleButtonOnClick"
      >
        Se innhold
      </button>
    </span>
    <br />
    <UiModal
      v-model="modalIsOpen"
      :title="fileName ?? ''"
      size="xl"
      ok-only
      ok-variant="secondary"
      ok-title="Lukk"
      @close="onClose"
    >
      <div v-if="content">
        <div v-if="isTextContent">
          <ssh-pre
            :language="fileExtension"
            :label="fileExtension?.toUpperCase() ?? ''"
          >
            {{ attemptDecodeBase64(content) }}
          </ssh-pre>
        </div>
        <div v-else>
          <iframe
            :src="getTemporaryUrl(content)"
            class="w-full h-96 border-0"
          />
        </div>
      </div>
      <div v-else>
        <UiSpinner label="Loading ..."></UiSpinner>
      </div>
    </UiModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import SshPre from 'simple-syntax-highlighter'
import 'simple-syntax-highlighter/dist/sshpre.css'
import UiModal from '@/components/ui/UiModal.vue'
import UiSpinner from '@/components/ui/UiSpinner.vue'

const mimeTypes: Record<string, string> = {
  pdf: 'application/pdf',
  xml: 'application/xml',
  json: 'application/json',
  txt: 'text/plain',
  html: 'text/html',
  htm: 'text/html',
  csv: 'text/csv',
  md: 'text/markdown',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  png: 'image/png',
  gif: 'image/gif',
  svg: 'image/svg+xml',
  zip: 'application/zip',
  doc: 'application/msword',
  docx: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  xls: 'application/vnd.ms-excel',
  xlsx: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
}

interface Props {
  fileName?: string
  content?: string | Blob
  protocol?: string
  testId?: string
  fileUrl?: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'get-content': [isTextContent: boolean]
}>()

const modalIsOpen = ref(false)
const fileExtension = ref<string | undefined>(undefined)
const isTextContent = ref(false)
const temporaryUrl = ref<string | null>(null)

onMounted(() => {
  fileExtension.value = getFileExtension(props.fileName)
  isTextContent.value = isTextFileExtension(fileExtension.value)
})

function handleButtonOnClick() {
  modalIsOpen.value = true
  if (!props.content) {
    emit('get-content', isTextContent.value)
  }
}

function getMimeType(extension?: string | null): string {
  return mimeTypes[extension?.toLowerCase() ?? ''] || 'application/octet-stream'
}

function getTemporaryUrl(content: string | Blob): string {
  const blob = getAsBlob(content)
  const url = URL.createObjectURL(blob)
  temporaryUrl.value = url
  return url
}

function getFileExtension(fileName?: string): string | undefined {
  if (fileName) {
    const arr = fileName.split('.')
    return arr[arr.length - 1]
  }
  return undefined
}

function isTextFileExtension(type: string | undefined): boolean {
  return ['xml', 'txt', 'json', 'html', 'csv', 'md'].includes(type ?? '')
}

function onClose() {
  if (temporaryUrl.value) {
    URL.revokeObjectURL(temporaryUrl.value)
    temporaryUrl.value = null
  }
}

function attemptDecodeBase64(content: string | Blob): string {
  if (typeof content !== 'string') return ''
  try {
    return atob(content)
  } catch {
    return content
  }
}

function getAsBlob(content: string | Blob): Blob {
  if (content instanceof Blob && content.type === 'application/octet-stream') {
    const contentType = fileExtension.value === 'pdf' ? { type: 'application/pdf' } : undefined
    return new Blob([content], contentType)
  }

  if (typeof content === 'string') {
    const decodedContent = atob(content)
    const binaryLen = decodedContent.length
    const bytes = new Uint8Array(binaryLen)

    for (let i = 0; i < binaryLen; i++) {
      bytes[i] = decodedContent.charCodeAt(i)
    }

    const mimeType = getMimeType(fileExtension.value)
    return new Blob([bytes], { type: mimeType })
  }

  return content as Blob
}

function getAsFile(content: string | Blob) {
  const blob = getAsBlob(content)
  const url = URL.createObjectURL(blob)
  temporaryUrl.value = url

  const a = document.createElement('a')
  document.body.appendChild(a)
  a.style.display = 'none'
  a.href = url
  a.download = props.fileName ?? 'download'
  a.click()
  URL.revokeObjectURL(url)
  document.body.removeChild(a)
}

function openWindow(link?: string) {
  if (!link) {
    if (props.content) {
      getAsFile(props.content)
    }
  } else {
    window.open(link, '_blank')
  }
}
</script>
