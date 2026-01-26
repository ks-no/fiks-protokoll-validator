/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, unknown>
  export default component
}

declare module 'simple-syntax-highlighter' {
  import { DefineComponent } from 'vue'
  const SshPre: DefineComponent<{
    language?: string
    label?: string
    dark?: boolean
    copyButton?: boolean
    reactive?: boolean
  }>
  export default SshPre
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly BASE_URL: string
  readonly MODE: string
  readonly DEV: boolean
  readonly PROD: boolean
  readonly SSR: boolean
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
