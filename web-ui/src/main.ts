import { createApp, type App as VueApp, type Plugin } from 'vue'
import App from '@/App.vue'
import StartPage from '@/components/StartPage.vue'
import TestSession from '@/components/TestSession.vue'
import NewTestSession from '@/components/NewTestSession.vue'
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import '@/assets/main.css'

import UIComponents from '@/components/ui'


const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'StartPage',
    component: StartPage
  },
  {
    path: '/TestSession/:testSessionId',
    name: 'testSession',
    component: TestSession
  },
  {
    path: '/NewTestSession',
    name: 'newTestSession',
    component: NewTestSession
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

const vueApp: VueApp = createApp(App)
vueApp.use(router)
vueApp.use(UIComponents as Plugin)
vueApp.mount('#app')
