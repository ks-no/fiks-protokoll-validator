// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import { createApp } from "vue";
import App from "@/App";
import { BootstrapVue, IconsPlugin } from "bootstrap-vue";
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import "bootstrap/dist/css/bootstrap.css";
import "bootstrap-vue/dist/bootstrap-vue.css";
import StartPage from "@/components/StartPage";
import TestSession from "@/components/TestSession";
import NewTestSession from "@/components/NewTestSession";
import {createRouter, createWebHistory} from "vue-router/dist/vue-router";

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
    history: createWebHistory(process.env.BASE_URL),
    routes,
})

const vueApp = createApp(App);
vueApp.use(router);
vueApp.use(BootstrapVue);
vueApp.use(IconsPlugin);
vueApp.mount("#app");
vueApp.component('font-awesome-icon', FontAwesomeIcon);
