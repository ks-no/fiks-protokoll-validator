import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'
import { createMockApiPlugin } from './mock-api.js'

export default defineConfig({
  plugins: [
    vue(),
    createMockApiPlugin()
  ],
  base: '/fiks-validator/',
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
