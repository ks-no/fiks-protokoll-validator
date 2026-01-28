import { ref } from 'vue'

interface UseClipboardReturn {
  copied: ReturnType<typeof ref<boolean>>
  copy: (text: string) => Promise<boolean>
}

export function useClipboard(): UseClipboardReturn {
  const copied = ref(false)

  async function copy(text: string): Promise<boolean> {
    try {
      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text)
        copied.value = true
        setTimeout(() => {
          copied.value = false
        }, 2000)
        return true
      }

      // Fallback for older browsers
      const textArea = document.createElement('textarea')
      textArea.value = text
      textArea.style.position = 'fixed'
      textArea.style.left = '-999999px'
      textArea.style.top = '-999999px'
      document.body.appendChild(textArea)
      textArea.focus()
      textArea.select()

      let success = false
      try {
        success = document.execCommand('copy')
      } finally {
        document.body.removeChild(textArea)
      }

      if (success) {
        copied.value = true
        setTimeout(() => {
          copied.value = false
        }, 2000)
      }

      return success
    } catch {
      copied.value = false
      return false
    }
  }

  return {
    copied,
    copy
  }
}
