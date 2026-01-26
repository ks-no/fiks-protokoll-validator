import { createApp } from "vue";
import App from "@/App.vue";
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import StartPage from "@/components/StartPage.vue";
import TestSession from "@/components/TestSession.vue";
import NewTestSession from "@/components/NewTestSession.vue";
import {createRouter, createWebHistory} from "vue-router";
import '@/assets/main.css'

// Import UI components
import UIComponents from '@/components/ui/index.js'

library.add(faCheck)

const routes = [
    {
        path: "/",
        name: "StartPage",
        component: StartPage
    },
    {
        path: "/TestSession/:testSessionId",
        name: "testSession",
        component: TestSession
    },
    {
        path: "/NewTestSession",
        name: "newTestSession",
        component: NewTestSession
    }]

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
})

const vueApp = createApp(App);
vueApp.use(router);
vueApp.use(UIComponents);
vueApp.component('font-awesome-icon', FontAwesomeIcon);
vueApp.mount("#app");
