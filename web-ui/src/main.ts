import { createApp, type App as VueApp, type Plugin } from 'vue'
import App from '@/App.vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCheck, faCircleCheck, faCircleExclamation, faChevronRight, faChevronDown, faFileCirclePlus, faFileCircleMinus } from '@fortawesome/free-solid-svg-icons'
import StartPage from '@/components/StartPage.vue'
import TestSession from '@/components/TestSession.vue'
import NewTestSession from '@/components/NewTestSession.vue'
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import '@/assets/main.css'

import UIComponents from '@/components/ui'

library.add(faCheck, faCircleCheck, faCircleExclamation, faChevronRight, faChevronDown, faFileCirclePlus, faFileCircleMinus)

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
vueApp.component('font-awesome-icon', FontAwesomeIcon)
vueApp.mount('#app')
